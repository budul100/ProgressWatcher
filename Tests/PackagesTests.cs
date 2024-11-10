using System;
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

            var childpackage = basePackage.GetPackage(2, "test");

            Assert.Throws<InvalidOperationException>(() => basePackage.GetPackage(2, "test"));
        }

        [Fact]
        public void ChildsCounter()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childpackage1 = basePackage.GetPackage(2, "Child1");

            childpackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.25);

            childpackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.5);

            Assert.Throws<InvalidOperationException>(() => childpackage1.NextStep());

            Assert.True(watcher.ProgressAll == 0.5);

            var childpackage2 = basePackage.GetPackage(2, "Child2");

            childpackage2.NextStep();

            Assert.True(watcher.ProgressAll == 0.75);

            childpackage2.NextStep();

            Assert.True(watcher.ProgressAll == 1);

            Assert.Throws<InvalidOperationException>(() => childpackage2.NextStep());

            Assert.True(watcher.ProgressAll == 1);
        }

        [Fact]
        public void ChildsDisposal()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childpackage1 = basePackage.GetPackage(2, "Child1");

            childpackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.25);

            childpackage1.Dispose();

            Assert.True(watcher.ProgressAll == 0.5);

            var childpackage2 = basePackage.GetPackage(2, "Child2");

            childpackage2.NextStep();

            Assert.True(watcher.ProgressAll == 0.75);

            childpackage2.Dispose();

            Assert.True(watcher.ProgressAll == 1);
        }

        [Fact]
        public void ChildsStatus()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test1");

            Assert.True(watcher.Status == "Test1");

            var childpackage1 = basePackage.GetPackage(2, "Test2");

            Assert.True(watcher.Status == "Test2");

            childpackage1.NextStep();

            Assert.True(watcher.Status == "Test2");

            childpackage1.NextStep();

            Assert.True(watcher.Status == "Test2");

            childpackage1.Dispose();

            Assert.True(watcher.Status == "Test1");

            var childpackage2 = basePackage.GetPackage(2, "Test3");

            Assert.True(watcher.Status == "Test3");

            childpackage2.NextStep();

            Assert.True(watcher.Status == "Test3");

            childpackage2.NextStep();

            Assert.True(watcher.Status == "Test3");

            childpackage2.Dispose();

            Assert.True(watcher.Status == "Test1");
        }

        [Fact]
        public void ChildsWeight()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Base");

            var childpackage1 = basePackage.GetPackage(2, "Child1", 0.8);

            childpackage1.NextStep();

            Assert.True(watcher.ProgressAll == 0.4);

            childpackage1.Dispose();

            Assert.True(watcher.ProgressAll == 0.8);

            var childpackage2 = basePackage.GetPackage(2, "Child2");

            childpackage2.NextStep();

            Assert.True(watcher.ProgressAll == 0.9);

            childpackage2.Dispose();

            Assert.True(watcher.ProgressAll == 1);
        }

        [Fact]
        public void TooMuchChilds()
        {
            var watcher = new Watcher();
            var basePackage = watcher.GetPackage(2, "Test");

            using (var childpackage = basePackage.GetPackage(2, "test"))
            { }

            using (var childpackage = basePackage.GetPackage(2, "test"))
            { }

            Assert.Throws<InvalidOperationException>(() => basePackage.GetPackage(2, "test"));
        }

        #endregion Public Methods
    }
}