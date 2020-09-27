using System;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public class MyDataFlow<TStep1Model, TStep2Model> : MyDataFlowBase
    {
        public MyDataFlow(IDataFlowQueue dataFlowQueue, IDataFlowSettings dataFlowSettings)
            :base(dataFlowQueue, dataFlowSettings)
        {
            _step1Model = typeof(TStep1Model);
            _step2Model = typeof(TStep2Model);
        }

        private Func<TStep1Model, int, TStep2Model> _step1Callback;
        private readonly Type _step1Model;
        
        private Action<TStep2Model, int> _step2Callback;
        private readonly Type _step2Model;

        protected override async ValueTask HandleStepModelAsync(object stepModel, int attemptNo, DateTime now)
        {
            if (stepModel.GetType() == _step1Model)
            {
                var modelResult = _step1Callback((TStep1Model) stepModel, attemptNo);
                await Enqueue(modelResult, now);
                await ReadQueueIterationAsync(now);
            }
            
            if (stepModel.GetType() == _step2Model)
                _step2Callback((TStep2Model) stepModel, attemptNo);
            
        }

        public MyDataFlow<TStep1Model, TStep2Model> WithStep1(Func<TStep1Model, int, TStep2Model> stepCallback)
        {
            _step1Callback = stepCallback;
            return this;
        }

        public StartFlowHelper<TStep1Model> WithStep2(Action<TStep2Model, int> stepCallback)
        {
            _step2Callback = stepCallback;
            return new StartFlowHelper<TStep1Model>(this);
        }
  
    }
}