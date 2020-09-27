using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public abstract class MyDataFlowBase
    {
        private readonly IDataFlowQueue _dataFlowQueue;
        private readonly IDataFlowSettings _dataFlowSettings;

        private Func<object, byte[]> _serializer;
        private Func<byte[], object> _deserializer;


        internal ValueTask Enqueue(object o, DateTime executeAt)
        {
            if (_serializer == null)
                throw new Exception("Please specify serializer");
            
            var stepModel = _serializer(o);
            return _dataFlowQueue.EnqueueAsync(stepModel, executeAt); 
        }

        protected MyDataFlowBase(IDataFlowQueue dataFlowQueue, IDataFlowSettings dataFlowSettings)
        {
            _dataFlowQueue = dataFlowQueue;
            _dataFlowSettings = dataFlowSettings;
        }
        
        public void RegisterSerializerDeserializer(Func<object, byte[]> serializer, Func<byte[], object> deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }
        

        private Action<object, int, object> _logCallback;

        public void RegisterLog(Action<object, int, object> logCallback)
        {
            _logCallback = logCallback;
        }


        private Action<object> _callbackFailMaxAttempts;
        public void RegisterStepFail(Action<object> callbackFailMaxAttempts)
        {
            _callbackFailMaxAttempts = callbackFailMaxAttempts;
        }
        
        private async ValueTask HandleStepModelAsync(object stepModel, int attemptNo, DateTime now)
        {

            foreach (var (stepType, stepCallBack) in _firstSteps)
            {
                if (stepModel.GetType() == stepType)
                {
                    var modelResult = stepCallBack(stepModel, attemptNo);

                    if (modelResult != null)
                    {
                        await Enqueue(modelResult, now);
                        await ReadQueueIterationAsync(now);
                    }
                    break;
                }
            }
            
            if (stepModel.GetType() == _lastStepType)
                _lastStep(stepModel, attemptNo);
        }



        private object Deserialize(IQueueItem item)
        {
            try
            {
                return _deserializer(item.Data);
            }
            catch (Exception e)
            {
                _logCallback?.Invoke(item, -1, e);
                throw;
            }
        }
        
        public async ValueTask ReadQueueIterationAsync(DateTime now)
        {
            var nextItem = await _dataFlowQueue.DequeueAsync(now);
            
            if (nextItem == null)
                return;

            var model = Deserialize(nextItem);
            
            try
            {
                await HandleStepModelAsync(model, nextItem.AttemptNo, now);
            }
            catch (Exception e)
            {
                _logCallback?.Invoke(model, nextItem.AttemptNo, e);
                
                if (nextItem.AttemptNo < _dataFlowSettings.MaximumAttempts)
                    await _dataFlowQueue.EnqueueBackAsync(nextItem, now.Add(_dataFlowSettings.RequeueTimeOut));
                else
                    _callbackFailMaxAttempts?.Invoke(model);
            }

        }

        private readonly Dictionary<Type, Func<object, int, object>> _firstSteps 
            = new Dictionary<Type, Func<object, int, object>>();
        
        private Action<object, int> _lastStep;
        private Type _lastStepType;

        private void RegisterType(Type type, Func<object, int, object> callback)
        {
            if (!_firstSteps.TryAdd(type, callback))
                throw new Exception($"Duplicated step model {type} for the flow {GetType()}");
        }

        protected void RegisterStep<TStepBefore, TStepAfter>(Func<TStepBefore, int, TStepAfter> callback)
        {
            RegisterType(typeof(TStepBefore), (model, no)=>callback((TStepBefore)model,  no));
        }
        
        protected void RegisterStep<TStepModel>(Action<TStepModel, int> callback)
        {
            _lastStepType = typeof(TStepModel);
            _lastStep = (model, no)=>callback((TStepModel)model,  no);
        }
        
        
    }
}