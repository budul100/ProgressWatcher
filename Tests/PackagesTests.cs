using System;
using System.Threading.Tasks;
using Xunit;

namespace ProgressWatcher.Tests
{
    public class PackagesTests
    {
        #region Public Methods

        [Fact]
        public void ChildNotCompleted()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test");

            var childPackage = basePackage.GetPackage(2, "test");

            Assert.Throws<InvalidOperationException>(() => basePackage.GetPackage(2, "test"));
        }

        [Fact]
        public void ChildsCounter()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childPackage1 = basePackage.GetPackage(2, "Child1");

            childPackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.25);

            childPackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.5);

            Assert.Throws<InvalidOperationException>(() => childPackage1.NextStep());

            Assert.True(watcher.ProgressAll == 0.5);

            var childPackage2 = basePackage.GetPackage(2, "Child2");

            childPackage2.NextStep();

            Assert.True(watcher.ProgressAll == 0.75);

            childPackage2.NextStep();

            Assert.True(watcher.ProgressAll == 1);

            Assert.Throws<InvalidOperationException>(() => childPackage2.NextStep());

            Assert.True(watcher.ProgressAll == 1);
        }

        [Fact]
        public void ChildsDisposal()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childPackage1 = basePackage.GetPackage(2, "Child1");

            childPackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.25);

            childPackage1.Dispose();

            Assert.True(watcher.ProgressAll == 0.5);

            var childPackage2 = basePackage.GetPackage(2, "Child2");

            childPackage2.NextStep();

            Assert.True(watcher.ProgressAll == 0.75);

            childPackage2.Dispose();

            Assert.True(watcher.ProgressAll == 1);
        }

        [Fact]
        public void ChildsStatus()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test1");

            Assert.True(watcher.Status == "Test1");

            var childPackage1 = basePackage.GetPackage(2, "Test2");

            Assert.True(watcher.Status == "Test2");

            childPackage1.NextStep();

            Assert.True(watcher.Status == "Test2");

            childPackage1.NextStep();

            Assert.True(watcher.Status == "Test2");

            childPackage1.Dispose();

            Assert.True(watcher.Status == "Test1");

            var childPackage2 = basePackage.GetPackage(2, "Test3");

            Assert.True(watcher.Status == "Test3");

            childPackage2.NextStep();

            Assert.True(watcher.Status == "Test3");

            childPackage2.NextStep();

            Assert.True(watcher.Status == "Test3");

            childPackage2.Dispose();

            Assert.True(watcher.Status == "Test1");
        }

        [Fact]
        public void ChildsWeight()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childPackage1 = basePackage.GetPackage(2, "Child1", 0.8);

            childPackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.4);

            childPackage1.Dispose();

            Assert.True(watcher.ProgressAll == 0.8);

            var childPackage2 = basePackage.GetPackage(2, "Child2");

            childPackage2.NextStep();

            Assert.True(watcher.ProgressAll == 0.9);

            childPackage2.Dispose();

            Assert.True(watcher.ProgressAll == 1);
        }

        [Fact]
        public void HandleProgress()
        {
            var watcher = new Watcher();

            var basePackage = watcher.GetPackage(2, "Test");

            var childProgress1 = basePackage.GetProgress();

            Task.Delay(1000).Wait();

            childProgress1.Report(0.5);

            Task.Delay(1000).Wait();

            Assert.True(watcher.ProgressAll == 0.25);

            childProgress1.Report(0.6);

            Task.Delay(1000).Wait();

            Assert.True(watcher.ProgressAll == 0.3);

            var childProgress2 = basePackage.GetProgress();

            Task.Delay(1000).Wait();

            childProgress2.Report(0.5);

            Task.Delay(1000).Wait();

            Assert.True(watcher.ProgressAll == 0.75);

            childProgress2.Report(0.6);

            Task.Delay(1000).Wait();

            Assert.True(watcher.ProgressAll == 0.8);

            childProgress2.Report(1.5);

            Task.Delay(1000).Wait();

            Assert.True(watcher.ProgressAll == 1);
        }

        [Fact]
        public void TooMuchChilds()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test");

            using (var childPackage = basePackage.GetPackage(2, "test"))
            { }

            using (var childPackage = basePackage.GetPackage(2, "test"))
            { }

            Assert.Throws<InvalidOperationException>(() => basePackage.GetPackage(2, "test"));
        }

        #endregion Public Methods
    }
}