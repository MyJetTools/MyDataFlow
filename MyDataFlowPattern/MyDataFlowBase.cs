using System;
using System.Threading.Tasks;

namespace MyDataFlowPattern
{
    public abstract class MyDataFlowBase
    {
        internal readonly IDataFlowQueue DataFlowQueue;
        internal readonly IDataFlowSettings DataFlowSettings;

        internal Func<object, byte[]> Serializer { get; private set; }
        internal Func<byte[], object> Deserializer{ get; private set; }

        internal ValueTask Enqueue(object o, DateTime executeAt)
        {
            if (Serializer == null)
                throw new Exception("Please specify serializer");
            
            var stepModel = Serializer(o);
            return DataFlowQueue.EnqueueAsync(stepModel, executeAt); 
        }

        protected MyDataFlowBase(IDataFlowQueue dataFlowQueue, IDataFlowSettings dataFlowSettings)
        {
            DataFlowQueue = dataFlowQueue;
            DataFlowSettings = dataFlowSettings;
        }
        
        public void RegisterSerializerDeserializer(Func<object, byte[]> serializer, Func<byte[], object> deserializer)
        {
            Serializer = serializer;
            Deserializer = deserializer;
        }

        protected abstract ValueTask HandleStepModelAsync(object stepModel, int attemptNo, DateTime now);
        
        public async ValueTask ReadQueueIterationAsync(DateTime now)
        {
            var nextItem = await DataFlowQueue.DequeueAsync(now);
            
            if (nextItem == null)
                return;

            try
            {
                var model = Deserializer(nextItem.Data);
                await HandleStepModelAsync(model, nextItem.AttemptNo, now);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await DataFlowQueue.EnqueueBackAsync(nextItem, now.Add(DataFlowSettings.RequeueTimeOut));
            }

        }
    }
}