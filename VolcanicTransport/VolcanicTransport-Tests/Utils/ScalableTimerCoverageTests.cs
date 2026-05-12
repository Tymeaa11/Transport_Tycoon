using VolcanicTransport.Model.Utils;

namespace VolcanicTransport_Tests.Utils
{
    [TestClass]
    public class ScalableTimerCoverageTests
    {
        [TestMethod]
        public void Enabled_GetAndSet_Work()
        {
            using var timer = new ScalableTimer();
            timer.Enabled = false;
            Assert.IsFalse(timer.Enabled);
            timer.Enabled = true;
            Assert.IsTrue(timer.Enabled);
            timer.Enabled = false;
        }

        [TestMethod]
        public void TimeScale_Getter_ReturnsSetValue()
        {
            using var timer = new ScalableTimer();
            timer.TimeScale = 2;
            Assert.AreEqual(2, timer.TimeScale);
            timer.TimeScale = 0;
        }

        [TestMethod]
        public void Stop_DoesNotThrow()
        {
            using var timer = new ScalableTimer();
            timer.TimeScale = 1;
            timer.Stop();
        }
    }
}
