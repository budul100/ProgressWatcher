using System;

namespace ProgressWatcher.Interfaces
{
    internal interface IParent : IDisposable
    {
        #region Public Methods

        void DisposeChild();

        void StartChild();

        void UpdateChild(double progress);

        void UpdateTip(string status);

        void UpdateTip(double progress);

        #endregion Public Methods
    }
}