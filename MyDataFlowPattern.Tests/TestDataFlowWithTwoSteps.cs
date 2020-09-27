using System;
using NUnit.Framework;

namespace MyDataFlowPattern.Tests
{
    public class TestDataFlowWithTwoSteps
    {
        [Test]
        public void TestDataFlowWithPositiveScenarios()
        {
            var ioc = TestUtils.GetDataFlow<DataFlowModelStep1, DataFlowModelStep2>(TimeSpan.FromSeconds(1));

            var dataFlow = ioc.GetService<MyDataFlow<DataFlowModelStep1, DataFlowModelStep2>>();
            
            DataFlowModelStep1 resultStep1 = null;
            var step1AttemptNo = -1;

            DataFlowModelStep2 resultStep2 = null;
            var step2AttemptNo = -1;

            var dt = DateTime.UtcNow;

            dataFlow
                .WithStep1((model, stepAttemptNo) =>
                {
                    resultStep1 = model;
                    step1AttemptNo = stepAttemptNo;
                    
                    return new DataFlowModelStep2
                    {
                        Value = model.IntValue.ToString() 
                    };
                })
                .WithStep2((model, attemptNo) =>
                {
                    resultStep2 = model;
                    step2AttemptNo = attemptNo;
                })
                .StartFlowAsync(new DataFlowModelStep1
                {
                    IntValue = 15
                }, dt)
                .AsTask()
                .Wait();

            Assert.AreEqual(-1, step1AttemptNo);
            Assert.IsNull(resultStep1);
            
            ioc.PushReadQueueTimer(dt);
            Assert.AreEqual(0, step1AttemptNo); 
            Assert.IsNotNull(resultStep1);
            Assert.AreEqual(15, resultStep1.IntValue);
            
            Assert.AreEqual(0, step2AttemptNo); 
            Assert.IsNotNull(resultStep2);
            Assert.AreEqual("15", resultStep2.Value); 
            
            
        }
    }
}