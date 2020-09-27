using System;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public interface IDataFlowSettings
    {
        TimeSpan RequeueTimeOut { get; }
    }
    public class MyDataFlow<TStepModel> : MyDataFlowBase
    {

        public MyDataFlow(IDataFlowQueue dataFlowQueue, IDataFlowSettings dataFlowSettings)
            :base(dataFlowQueue, dataFlowSettings)
        {

        }

        public StartFlowHelper<TStepModel> WithStep(Action<TStepModel, int> stepCallback)
        {
            RegisterStep(stepCallback);
            return new StartFlowHelper<TStepModel>(this);
        }


    }
    
}