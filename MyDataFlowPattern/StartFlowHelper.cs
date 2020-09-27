using System;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public class StartFlowHelper<TStartModel> 
    {
        private readonly MyDataFlowBase _myDataFlowBase;

        public StartFlowHelper(MyDataFlowBase myDataFlowBase)
        {
            _myDataFlowBase = myDataFlowBase;
        }

        public ValueTask StartFlowAsync(TStartModel firstStep, DateTime executeAt)
        {
            return _myDataFlowBase.Enqueue(firstStep, executeAt);
        }
    }
}