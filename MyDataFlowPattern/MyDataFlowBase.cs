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

        private async ValueTask HandleStepModelAsync(object stepModel, int attemptNo, DateTime now)
        {

            foreach (var (stepType, stepCallBack) in _firstSteps)
            {
                if (stepModel.GetType() == stepType)
                {
                    var modelResult = stepCallBack(stepModel, attemptNo);
                    await Enqueue(modelResult, now);
                    await ReadQueueIterationAsync(now);
                    break;
                }
            }
            
            if (stepModel.GetType() == _lastStepType)
                _lastStep(stepModel, attemptNo);
        }

        public async ValueTask ReadQueueIterationAsync(DateTime now)
        {
            var nextItem = await _dataFlowQueue.DequeueAsync(now);
            
            if (nextItem == null)
                return;

            try
            {
                var model = _deserializer(nextItem.Data);
                await HandleStepModelAsync(model, nextItem.AttemptNo, now);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await _dataFlowQueue.EnqueueBackAsync(nextItem, now.Add(_dataFlowSettings.RequeueTimeOut));
            }

        }

        private readonly Dictionary<Type, Func<object, int, object>> _firstSteps 
            = new Dictionary<Type, Func<object, int, object>>();
        
        private Action<object, int> _lastStep;
        private Type _lastStepType;

        private void RegisterType(Type type, Func<object, int, object> callback)
        {
            _firstSteps.Add(type, callback);
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