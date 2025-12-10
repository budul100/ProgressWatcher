using System;
using System.Linq;
using System.Threading.Tasks;
using ProgressWatcher;
using Xunit;

namespace ProgressWatcherTest
{
    public class WatcherTests
    {
        #region Public Methods

        [Fact]
        public void Dispose_WatcherResetsState()
        {
            var watcher = new Watcher();

            using var basePackage = watcher.GetPackage(2, "Base");

            Assert.True(watcher.IsRunning, "Watcher should be running after GetPackage.");

            // Dispose watcher (public API) should reset internal running state
            watcher.Dispose();

            Assert.False(watcher.IsRunning, "Watcher should not be running after Dispose.");
            Assert.Equal(0d, watcher.ProgressAll);
            Assert.Equal(0d, watcher.ProgressTip);
            Assert.Null(watcher.Status);
        }

        [Fact]
        public void GetPackage_ThrowsWhenPackageAlreadyActive()
        {
            var watcher = new Watcher();

            var basePackage = watcher.GetPackage(2, "Base");

            // Asking for another base package while one is active should throw
            Assert.Throws<InvalidOperationException>(() => watcher.GetPackage(1, "Another"));
        }

        [Fact]
        public async Task LifecycleEvents_AreRaised()
        {
            var watcher = new Watcher();

            var progressStartedTcs = new TaskCompletionSource<bool>();
            var childStartedTcs = new TaskCompletionSource<bool>();
            var childDisposedTcs = new TaskCompletionSource<bool>();
            var progressCompletedTcs = new TaskCompletionSource<bool>();

            watcher.WatcherStarted += (s, e) => progressStartedTcs.TrySetResult(true);
            watcher.ChildStarted += (s, e) => childStartedTcs.TrySetResult(true);
            watcher.ChildCompleted += (s, e) => childDisposedTcs.TrySetResult(true);
            watcher.WatcherCompleted += (s, e) => progressCompletedTcs.TrySetResult(true);

            // Start base package -> should raise ProgressStarted
            using (var basePackage = watcher.GetPackage(1, "base"))
            {
                var started = await Task.WhenAny(progressStartedTcs.Task, Task.Delay(1000));
                Assert.True(progressStartedTcs.Task.IsCompleted, "ProgressStarted should be raised when a package is requested.");

                // Create child package -> should raise ChildStarted
                using (var child = basePackage.GetPackage(1, "child"))
                {
                    var childStarted = await Task.WhenAny(childStartedTcs.Task, Task.Delay(1000));
                    Assert.True(childStartedTcs.Task.IsCompleted, "ChildStarted should be raised when a child package starts.");
                }
                await Task.Delay(100);

                var disposed = await Task.WhenAny(childDisposedTcs.Task, Task.Delay(1000));
                Assert.True(childDisposedTcs.Task.IsCompleted, "ChildDisposed should be raised when child package is disposed.");
            }

            var completed = await Task.WhenAny(progressCompletedTcs.Task, Task.Delay(1000));
            Assert.True(progressCompletedTcs.Task.IsCompleted, "ProgressCompleted should be raised after child disposal.");
        }

        [Fact]
        public async Task ParallelChildCreateDispose_Resilient()
        {
            var watcher = new Watcher();
            var totalSteps = 1_000;
            using var basePackage = watcher.GetPackage(totalSteps, "parallel-children");

            var tasks = Enumerable.Range(0, Environment.ProcessorCount * 2)
                .Select(_ => Task.Run(async () =>
                {
                    // Each worker repeatedly tries to create+dispose single-step children
                    for (var attempts = 0; attempts < totalSteps; attempts++)
                    {
                        if (watcher.ProgressAll >= 1.0) break;

                        try
                        {
                            using var child = basePackage.GetPackage(1, "child");

                            // immediate disposal increments the parent step
                        }
                        catch (InvalidOperationException)
                        {
                            // races may cause transient InvalidOperationException when a child slot is unavailable;
                            // retry after yielding to other tasks
                            await Task.Yield();
                        }
                    }
                }))
                .ToArray();

            await Task.WhenAll(tasks);

            // Allow any pending updates to propagate
            await Task.Delay(3000);

            Assert.InRange(watcher.ProgressAll, 0.999, 1.0);
        }

        [Fact]
        public async Task ParallelNextStep_AggregatesCorrectly()
        {
            var watcher = new Watcher();
            var totalSteps = 5_000;
            using var basePackage = watcher.GetPackage(totalSteps, "parallel-nextstep");

            // Advance steps in parallel using the public API
            Parallel.For(0, totalSteps, i =>
            {
                basePackage.NextStep();
            });

            await Task.Delay(200);

            Assert.InRange(watcher.ProgressAll, 0.9999, 1.0);
        }

        [Fact]
        public async Task ParallelNextStepLocal_AggregatesCorrectly()
        {
            var totalSteps = 10_000;

            using var watcher = new Watcher();
            using var basePackage = watcher.GetPackage(totalSteps, "parallel-local");

            // Run many thread-local step reports in parallel
            Parallel.For(0, totalSteps, i =>
            {
                basePackage.NextStepLocal();
            });

            // Flush remaining thread-local counters and allow aggregation to settle
            basePackage.FlushCounterLocal();
            await Task.Delay(200);

            // Expect full completion
            Assert.InRange(watcher.ProgressAll, 0.9999, 1.0);
        }

        [Fact]
        public async Task ProgressReporter_AggregatesToProgressAll()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test");

            // Get the progress reporter from the public package API (no internals)
            var reporter = basePackage.GetProgress();

            // Report half of a single child; allow small time for aggregation if necessary
            reporter.Report(0.5);
            await Task.Delay(200);

            // For two steps total, 0.5 of the first step => 0.25 overall
            Assert.Equal(0.25, watcher.ProgressAll);

            // Report completion of that step; progress should increase
            reporter.Report(1.0);
            await Task.Delay(200);

            // Now the first step counted fully -> 0.5 overall
            Assert.Equal(0.5, watcher.ProgressAll);
        }

        #endregion Public Methods
    }
}
