using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public class DataFlowQueueInMemory : IDataFlowQueue
    {
        private class QueueItem : IQueueItem
        {

            private static long _nextItemId; 

            public QueueItem()
            {
                Id = _nextItemId;
                _nextItemId++;
            }
            
            public long Id { get; }
            public int AttemptNo { get; internal set; }
            public byte[] Data { get; set; }
            public DateTime VisibleAt{ get; set; }
        }
        
        private readonly SortedDictionary<long, QueueItem> _items = new SortedDictionary<long, QueueItem>();
        
        public ValueTask EnqueueAsync(byte[] data, DateTime visibleAt)
        {

            lock (_items)
            {
                var newItem = new QueueItem
                {
                    Data = data,
                    VisibleAt = visibleAt
                };
                
                _items.Add(newItem.Id, newItem);
            }

            return new ValueTask();
        }

        public ValueTask EnqueueBackAsync(IQueueItem enqueueBackItem, DateTime visibleAt)
        {
            lock (_items)
            {
                var theItem = enqueueBackItem as QueueItem ??  new QueueItem
                {
                    Data = enqueueBackItem.Data,
                    VisibleAt = visibleAt,
                    AttemptNo = enqueueBackItem.AttemptNo
                };

                theItem.AttemptNo++;
                
                _items.Add(theItem.Id, theItem);
            }
            
            return new ValueTask();
        }

        public ValueTask<IQueueItem> DequeueAsync(DateTime nowDate)
        {

            lock (_items)
            {
                var nextItem = _items.Values.FirstOrDefault(itm => itm.VisibleAt >= nowDate);

                if (nextItem != null)
                {
                    _items.Remove(nextItem.Id);
                    return new ValueTask<IQueueItem>(nextItem);

                }
            }

            return new ValueTask<IQueueItem>();
        }
    }
}