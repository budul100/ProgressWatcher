using System;
using System.Linq;
using System.Threading.Tasks;
using ProgressWatcher;
using Xunit;

namespace ProgressWatcherTest
{
    public class PackagesTests
    {
        #region Public Methods

        /// <summary>
        /// Verifies that disposing child packages updates the overall progress as expected.
        /// </summary>
        [Fact]
        public void ChildrenDisposal_UpdatesProgress()
        {
            using var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childPackage1 = basePackage.GetPackage(2, "Child1");

            childPackage1.NextStep();

            Assert.Equal(0.25, watcher.ProgressAll);

            childPackage1.Dispose();

            Assert.Equal(0.5, watcher.ProgressAll);

            var childPackage2 = basePackage.GetPackage(2, "Child2");

            childPackage2.NextStep();

            Assert.Equal(0.75, watcher.ProgressAll);

            childPackage2.Dispose();

            Assert.Equal(1, watcher.ProgressAll);
        }

        /// <summary>
        /// Verifies progress aggregation when children advance through their steps.
        /// Ensures progress increases correctly and further NextStep calls throw when completed.
        /// </summary>
        [Fact]
        public void ChildrenProgress_CalculationIsCorrect()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            using (var childPackage1 = basePackage.GetPackage(2, "Child1"))
            {
                childPackage1.NextStep();

                Assert.Equal(0.25, watcher.ProgressAll);

                childPackage1.NextStep();

                Assert.Equal(0.5, watcher.ProgressAll);

                Assert.Throws<InvalidOperationException>(childPackage1.NextStep);

                Assert.Equal(0.5, watcher.ProgressAll);
            }

            using (var childPackage2 = basePackage.GetPackage(2, "Child2"))
            {
                childPackage2.NextStep();

                Assert.Equal(0.75, watcher.ProgressAll);

                childPackage2.NextStep();

                Assert.Equal(1, watcher.ProgressAll);

                Assert.Throws<InvalidOperationException>(() => childPackage2.NextStep());

                Assert.Equal(1, watcher.ProgressAll);
            }
        }

        /// <summary>
        /// Verifies that the Watcher.Status reflects the currently active child's status and reverts to the parent's status after disposal.
        /// </summary>
        [Fact]
        public void ChildrenStatus_ReflectsActiveChild()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test1");

            Assert.Equal("Test1", watcher.Status);

            var childPackage1 = basePackage.GetPackage(2, "Test2");

            Assert.Equal("Test2", watcher.Status);

            childPackage1.NextStep();

            Assert.Equal("Test2", watcher.Status);

            childPackage1.NextStep();

            Assert.Equal("Test2", watcher.Status);

            childPackage1.Dispose();

            Assert.Equal("Test1", watcher.Status);

            var childPackage2 = basePackage.GetPackage(2, "Test3");

            Assert.Equal("Test3", watcher.Status);

            childPackage2.NextStep();

            Assert.Equal("Test3", watcher.Status);

            childPackage2.NextStep();

            Assert.Equal("Test3", watcher.Status);

            childPackage2.Dispose();

            Assert.Equal("Test1", watcher.Status);
        }

        /// <summary>
        /// Verifies that child weights affect the overall progress proportionally.
        /// </summary>
        [Fact]
        public void ChildrenWeight_InfluencesOverallProgress()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childPackage1 = basePackage.GetPackage(2, "Child1", 0.8);

            childPackage1.NextStep();

            Assert.Equal(0.4, watcher.ProgressAll);

            childPackage1.Dispose();

            Assert.Equal(0.8, watcher.ProgressAll);

            var childPackage2 = basePackage.GetPackage(2, "Child2");

            childPackage2.NextStep();

            Assert.Equal(0.9, watcher.ProgressAll);

            childPackage2.Dispose();

            Assert.Equal(1, watcher.ProgressAll);
        }

        /// <summary>
        /// Verifies that requesting a child package with the same name twice throws an InvalidOperationException.
        /// </summary>
        [Fact]
        public void GetPackage_ThrowsWhenChildAlreadyExists()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test");

            var childPackage = basePackage.GetPackage(2, "test");

            Assert.Throws<InvalidOperationException>(() => basePackage.GetPackage(2, "test"));
        }

        /// <summary>
        /// Verifies that creating too many children with the same name is prevented and triggers an InvalidOperationException.
        /// </summary>
        [Fact]
        public void GetPackage_ThrowsWhenTooManyChildren()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test");

            using (var childPackage = basePackage.GetPackage(2, "test"))
            { }

            using (var childPackage = basePackage.GetPackage(2, "test"))
            { }

            Assert.Throws<InvalidOperationException>(() => basePackage.GetPackage(2, "test"));
        }

        /// <summary>
        /// Verifies that progress reports from child progress reporters are properly reflected in the Watcher after propagation.
        /// </summary>
        [Fact]
        public async Task ProgressReporting_ChildProgressUpdatesWatcher()
        {
            var watcher = new Watcher();

            var basePackage = watcher.GetPackage(2, "Test");

            var childProgress1 = basePackage.GetProgress();

            await Task.Delay(1000);

            childProgress1.Report(0.5);

            await Task.Delay(1000);

            Assert.Equal(0.25, watcher.ProgressAll);

            childProgress1.Report(0.6);

            await Task.Delay(1000);

            Assert.Equal(0.3, watcher.ProgressAll);

            var childProgress2 = basePackage.GetProgress();

            await Task.Delay(1000);

            childProgress2.Report(0.5);

            await Task.Delay(1000);

            Assert.Equal(0.75, watcher.ProgressAll);

            childProgress2.Report(0.6);

            await Task.Delay(1000);

            Assert.Equal(0.8, watcher.ProgressAll);

            childProgress2.Report(1.5);

            await Task.Delay(1000);

            Assert.Equal(1, watcher.ProgressAll);
        }

        #endregion Public Methods
    }
}
