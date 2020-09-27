using System;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{

    public interface IQueueItem
    {
        int AttemptNo { get;}
        byte[] Data { get; }
    }
    
    public interface IDataFlowQueue
    {
        ValueTask EnqueueAsync(byte[] data, DateTime visibleAt);
        
        ValueTask EnqueueBackAsync(IQueueItem enqueueBackItem, DateTime visibleAt);
        ValueTask<IQueueItem> DequeueAsync(DateTime nowDate);
    }
}