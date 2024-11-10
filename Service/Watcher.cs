using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ProgressWatcher.Interfaces;

namespace ProgressWatcher
{
    public class Watcher
        : INotifyPropertyChanged, IParent
    {
        #region Private Fields

        private bool isDisposed;
        private bool isRunning;
        private IPackage package;
        private double progressFull;
        private double progressTip;
        private string status;

        #endregion Private Fields

        #region Public Events

        public event EventHandler ChildDisposed;

        public event EventHandler ChildStarted;

        public event EventHandler ProgressCompleted;

        public event EventHandler ProgressStarted;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public bool IsRunning
        {
            get => isRunning;
            private set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ProgressAll
        {
            get => progressFull;
            private set
            {
                if (progressFull != value)
                {
                    progressFull = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ProgressTip
        {
            get => progressTip;
            set
            {
                progressTip = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => status;
            private set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IParent.DisposeChild()
        {
            package = default;

            Status = default;
            ProgressTip = 0;
            ProgressAll = 0;
            IsRunning = false;

            ChildDisposed?.Invoke(
                sender: this,
                e: default);

            OnProgressCompleted();
        }

        public virtual IPackage Initialize(int allSteps, string status)
        {
            if (package != default)
            {
                throw new InvalidOperationException("There is already a base progress package.");
            }

            Status = default;
            ProgressTip = 0;
            ProgressAll = 0;
            IsRunning = true;

            package = new Package(
                parent: this,
                status: status,
                steps: allSteps);

            OnProgressRequested();

            return package;
        }

        public virtual IPackage Initialize(IEnumerable<object> allItems, string status)
        {
            Initialize(
                allSteps: allItems?.Count() ?? 0,
                status: status);

            return package;
        }

        void IParent.StartChild()
        {
            ChildStarted?.Invoke(
                sender: this,
                e: default);
        }

        void IParent.UpdateChild(double progress)
        {
            ProgressAll = progress;
        }

        void IParent.UpdateTip(string status)
        {
            Status = status;
        }

        void IParent.UpdateTip(double progress)
        {
            ProgressTip = progress;
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    ((IParent)this).DisposeChild();
                }

                isDisposed = true;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = default)
        {
            PropertyChanged?.Invoke(
                sender: this,
                e: new PropertyChangedEventArgs(propertyName));
        }

        #endregion Protected Methods

        #region Private Methods

        private void OnProgressCompleted()
        {
            ProgressCompleted?.Invoke(
                sender: this,
                e: default);
        }

        private void OnProgressRequested()
        {
            ProgressStarted?.Invoke(
                sender: this,
                e: default);
        }

        #endregion Private Methods
    }
}