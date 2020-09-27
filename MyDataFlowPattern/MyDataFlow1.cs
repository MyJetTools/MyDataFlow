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
            _stepType = typeof(TStepModel);
        }

        private Action<TStepModel, int> _step;
        private readonly Type _stepType;

        public StartFlowHelper<TStepModel> WithStep(Action<TStepModel, int> stepCallback)
        {
            _step = stepCallback;
            return new StartFlowHelper<TStepModel>(this);
        }

        protected override ValueTask HandleStepModelAsync(object stepModel, int attemptNo, DateTime now)
        {
            if (stepModel.GetType() == _stepType)
                _step((TStepModel) stepModel, attemptNo);
            
            return new ValueTask();
        }

    }
    
}