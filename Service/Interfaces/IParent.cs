using System;

namespace ProgressWatcher.Interfaces
{
    internal interface IParent
        : IDisposable
    {
        #region Internal Methods

        internal void DisposeChild();

        internal void StartChild();

        internal void UpdateChild(double progress);

        internal void UpdateTip(string status);

        internal void UpdateTip(double progress);

        #endregion Internal Methods
    }
}