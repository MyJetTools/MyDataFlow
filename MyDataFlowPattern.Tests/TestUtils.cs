using System;
using MyDependencies;

namespace MyDataFlowPattern.Tests
{

    internal class DataFlowTestSettings : IDataFlowSettings
    {
        public TimeSpan RequeueTimeOut { get; set; }
        public int MaximumAttempts { get; } = 5;
    }
    
    public static class TestUtils
    {

        public static MyIoc GetDataFlow<T>(TimeSpan reQueueTimeOut)
        {
            var result = new MyIoc();
            
            var queue = new DataFlowQueueInMemory();
            var dataFlowSettings = new DataFlowTestSettings
            {
                RequeueTimeOut = reQueueTimeOut
            };

            var dataFlow = new MyDataFlow<T>(queue, dataFlowSettings);
            dataFlow.RegisterJsonSerializer();
            
            result.Register(dataFlow);
            result.Register<MyDataFlowBase>(dataFlow);
            
            
            return result;
        }
        
        
        public static MyIoc GetDataFlow<TStep1, TStep2>(TimeSpan reQueueTimeOut)
        {
            var result = new MyIoc();
            
            var queue = new DataFlowQueueInMemory();
            var dataFlowSettings = new DataFlowTestSettings
            {
                RequeueTimeOut = reQueueTimeOut
            };

            var dataFlow = new MyDataFlow<TStep1, TStep2>(queue, dataFlowSettings);
            dataFlow.RegisterJsonSerializer();
            
            result.Register(dataFlow);
            result.Register<MyDataFlowBase>(dataFlow);
            
            return result;
        }
        
        public static MyIoc GetDataFlow<TStep1, TStep2, TStep3>(TimeSpan reQueueTimeOut)
        {
            var result = new MyIoc();
            
            var queue = new DataFlowQueueInMemory();
            var dataFlowSettings = new DataFlowTestSettings
            {
                RequeueTimeOut = reQueueTimeOut
            };

            var dataFlow = new MyDataFlow<TStep1, TStep2, TStep3>(queue, dataFlowSettings);
            dataFlow.RegisterJsonSerializer();
            
            result.Register(dataFlow);
            result.Register<MyDataFlowBase>(dataFlow);
            
            return result;
        }

        
        
        public static void PushReadQueueTimer(this MyIoc ioc, DateTime utcNow)
        {
            var dataFlowTimer = ioc.GetService<MyDataFlowBase>();
            dataFlowTimer.ReadQueueIterationAsync(utcNow).AsTask().Wait();
        }
        
    }
}