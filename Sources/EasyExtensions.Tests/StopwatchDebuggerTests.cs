using EasyExtensions.Debuggers;

namespace EasyExtensions.Tests
{
    internal class StopwatchDebuggerTests
    {
        readonly StopwatchDebugger debugger;
        string action = string.Empty;
        TimeSpan elapsed = TimeSpan.Zero;

        public StopwatchDebuggerTests()
        {
            debugger = new StopwatchDebugger(x =>
            {
                action = x.Action;
                elapsed = x.Elapsed;
            });
        }

        [Test]
        public void CreateDebugger_ValidInput_ValidOutput()
        {
            Task.Delay(1000).Wait();
            debugger.Report("Started");
            Assert.Multiple(() =>
            {
                Assert.That(action, Is.EqualTo("Started"));
                Assert.That(elapsed.TotalSeconds, Is.GreaterThan(0.9).And.LessThan(1.1));
            });
        }
    }
}
