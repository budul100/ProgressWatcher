using System;

namespace ProgressWatcherTest.Sample
{
    public class SampleService
        : ProgressWatcher.Watcher, IDisposable
    {
        #region Private Fields

        private bool isDisposed;

        #endregion Private Fields

        #region Public Methods

        public override void Dispose()
        {
            Dispose(true);

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
            }
        }

        #endregion Protected Methods
    }
}