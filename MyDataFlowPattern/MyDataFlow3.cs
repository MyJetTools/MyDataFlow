using System;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public class MyDataFlow<TStep1Model, TStep2Model, TStep3Model> : MyDataFlowBase
    {
        public MyDataFlow(IDataFlowQueue dataFlowQueue, IDataFlowSettings dataFlowSettings)
            :base(dataFlowQueue, dataFlowSettings)
        {

        }

        public MyDataFlow<TStep1Model, TStep2Model, TStep3Model> WithStep1(Func<TStep1Model, int, TStep2Model> stepCallBack)
        {
            RegisterStep(stepCallBack);
            return this;
        }

        public MyDataFlow<TStep1Model, TStep2Model, TStep3Model> WithStep2(Func<TStep2Model, int, TStep3Model> stepCallBack)
        {
            RegisterStep(stepCallBack);
            return this;
        }
        
        public StartFlowHelper<TStep1Model> WithStep3(Action<TStep3Model, int> stepCallBack)
        {
            RegisterStep(stepCallBack);
            return new StartFlowHelper<TStep1Model>(this);
        }
    }
}