using Xunit;

namespace ProgressWatcherTest
{
    public class ServiceTests
    {
        #region Public Methods

        [Fact]
        public void DisposeService()
        {
            using var service = new Sample.SampleService();
            {
                Assert.True(service != default);
            }
        }

        #endregion Public Methods
    }
}