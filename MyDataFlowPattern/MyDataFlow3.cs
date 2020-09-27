using System;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public class MyDataFlow<TStep1Model, TStep2Model, TStep3Model> : MyDataFlowBase
    {
        public MyDataFlow(IDataFlowQueue dataFlowQueue, IDataFlowSettings dataFlowSettings)
            :base(dataFlowQueue, dataFlowSettings)
        {
            _step1Model = typeof(TStep1Model);
            _step2Model = typeof(TStep2Model);
            _step3Model = typeof(TStep3Model);
        }

        private Func<TStep1Model, int, TStep2Model> _step1Callback;
        private readonly Type _step1Model;
        
        private Func<TStep2Model, int, TStep3Model> _step2Callback;
        private readonly Type _step2Model;
        
        private Action<TStep3Model, int> _step3Callback;
        private readonly Type _step3Model;


        protected override async ValueTask HandleStepModelAsync(object stepModel, int attemptNo, DateTime now)
        {
            if (stepModel.GetType() == _step1Model)
            {
                var modelResult = _step1Callback((TStep1Model) stepModel, attemptNo);
                await Enqueue(modelResult, now);
                await ReadQueueIterationAsync(now);
            }
            
            if (stepModel.GetType() == _step2Model)
            {
                var modelResult = _step2Callback((TStep2Model) stepModel, attemptNo);
                await Enqueue(modelResult, now);
                await ReadQueueIterationAsync(now);
            }
            
            if (stepModel.GetType() == _step3Model)
                _step3Callback((TStep3Model) stepModel, attemptNo);
            
        }


        public MyDataFlow<TStep1Model, TStep2Model, TStep3Model> WithStep1(Func<TStep1Model, int, TStep2Model> stepCallBack)
        {
            _step1Callback = stepCallBack;
            return this;
        }

        public MyDataFlow<TStep1Model, TStep2Model, TStep3Model> WithStep2(Func<TStep2Model, int, TStep3Model> stepCallBack)
        {
            _step2Callback = stepCallBack;
            return this;
        }
        
        public StartFlowHelper<TStep1Model> WithStep3(Action<TStep3Model, int> stepCallBack)
        {
            _step3Callback = stepCallBack;
            return new StartFlowHelper<TStep1Model>(this);
        }
    }
}