using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentTestExtension;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public sealed class SingleRunningDelegateLauncherFixture
    {
        [TestMethod]
        public void WhenLauncherHasQueuedDelegate_ItShouldCompleted()
        {
            var sut = new SingleRunningDelegateLauncher();
            var tcs = new AsyncTaskCompletionSource();

            sut.Launch(ct => { tcs.SetResult(); }, t => { });

            ConcurrentAssert.EnsureThatTaskIsCompleted(tcs.Task);
        }
    }
}
