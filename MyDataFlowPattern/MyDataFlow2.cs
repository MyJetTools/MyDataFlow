using System;

namespace MyDataFlowPattern
{
    public class MyDataFlow<TStep1Model, TStep2Model> : MyDataFlowBase
    {
        public MyDataFlow(IDataFlowQueue dataFlowQueue, IDataFlowSettings dataFlowSettings)
            :base(dataFlowQueue, dataFlowSettings)
        {

        }

        public MyDataFlow<TStep1Model, TStep2Model> WithStep1(Func<TStep1Model, int, TStep2Model> stepCallback)
        {
            RegisterStep(stepCallback);
            return this;
        }

        public StartFlowHelper<TStep1Model> WithStep2(Action<TStep2Model, int> stepCallback)
        {
            RegisterStep(stepCallback);
            return new StartFlowHelper<TStep1Model>(this);
        }
  
    }
}