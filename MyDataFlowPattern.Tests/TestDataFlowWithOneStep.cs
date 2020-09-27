using System;
using NUnit.Framework;

namespace MyDataFlowPattern.Tests
{
    public class TestsDataFlowOneParameter
    {

        [Test]
        public void TestDataFlowWithOneStep()
        {
            var ioc = TestUtils.GetDataFlow<DataFlowModelStep1>(TimeSpan.FromSeconds(1));

            var dataFlow = ioc.GetService<MyDataFlow<DataFlowModelStep1>>();
            DataFlowModelStep1 result = null;
            var attemptNo = -1;

            var dt = DateTime.UtcNow;

            dataFlow
                .WithStep((model, stepAttemptNo) =>
                {
                    result = model;
                    attemptNo = stepAttemptNo;
                })
                .StartFlowAsync(new DataFlowModelStep1
                {
                    IntValue = 15
                }, dt)
                .AsTask()
                .Wait();

            Assert.AreEqual(-1, attemptNo);
            Assert.IsNull(result);
            
            ioc.PushReadQueueTimer(dt);
            Assert.AreEqual(0, attemptNo); 
            Assert.IsNotNull(result);
            Assert.AreEqual(15, result.IntValue); 
        }
    }
}