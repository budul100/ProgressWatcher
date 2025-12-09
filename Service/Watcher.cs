using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ProgressWatcher.Interfaces;

namespace ProgressWatcher
{
    /// <summary>
    /// Tracks progress for operations and provides child progress packages.
    /// Implements <see cref="INotifyPropertyChanged"/> and <see cref="IParent"/> to coordinate child progress updates.
    /// </summary>
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

        /// <summary>
        /// Raised when a child package has been disposed and the watcher has been reset.
        /// </summary>
        public event EventHandler ChildDisposed;

        /// <summary>
        /// Raised when a child package signals it has started work.
        /// </summary>
        public event EventHandler ChildStarted;

        /// <summary>
        /// Raised when progress has completed (child disposed or finished).
        /// </summary>
        public event EventHandler ProgressCompleted;

        /// <summary>
        /// Raised when progress is requested / started for a new package.
        /// </summary>
        public event EventHandler ProgressStarted;

        /// <summary>
        /// Standard property-changed event used by data-binding consumers.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether a child package is currently running.
        /// </summary>
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

        /// <summary>
        /// Gets the aggregate progress value for the current package (0..steps).
        /// </summary>
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

        /// <summary>
        /// Gets or sets the current tip progress (fine-grained progress inside the current step).
        /// </summary>
        public double ProgressTip
        {
            get => progressTip;
            set
            {
                progressTip = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the current status text for the running package.
        /// </summary>
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

        /// <summary>
        /// Disposes the watcher and any active child package.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the active child package and resets watcher state.
        /// This is the explicit implementation for <see cref="IParent.DisposeChild"/>.
        /// </summary>
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

        /// <summary>
        /// Creates and returns a new progress package with the specified number of steps and initial status.
        /// Throws <see cref="InvalidOperationException"/> if a package is already active.
        /// </summary>
        /// <param name="allSteps">Total number of steps for the package.</param>
        /// <param name="status">Initial status text for the package.</param>
        /// <returns>The created <see cref="IPackage"/> instance.</returns>
        public virtual IPackage GetPackage(int allSteps, string status)
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

        /// <summary>
        /// Creates and returns a new progress package for the provided collection of items.
        /// The package step count will be set to the number of items.
        /// </summary>
        /// <param name="allItems">Collection of items that defines the number of steps.</param>
        /// <param name="status">Initial status text for the package.</param>
        /// <returns>The created <see cref="IPackage"/> instance.</returns>
        public virtual IPackage GetPackage(IEnumerable<object> allItems, string status)
        {
            GetPackage(
                allSteps: allItems?.Count() ?? 0,
                status: status);

            return package;
        }

        /// <summary>
        /// Notifies that a child package has started. Explicit implementation of <see cref="IParent.StartChild"/>.
        /// </summary>
        void IParent.StartChild()
        {
            ChildStarted?.Invoke(
                sender: this,
                e: default);
        }

        /// <summary>
        /// Updates the aggregate progress reported by a child package.
        /// Explicit implementation of <see cref="IParent.UpdateChild"/>.
        /// </summary>
        /// <param name="progress">New aggregate progress value.</param>
        void IParent.UpdateChild(double progress)
        {
            ProgressAll = progress;
        }

        /// <summary>
        /// Updates the current status text reported by a child package.
        /// Explicit implementation of <see cref="IParent.UpdateTip(string)"/>.
        /// </summary>
        /// <param name="status">New status text.</param>
        void IParent.UpdateTip(string status)
        {
            Status = status;
        }

        /// <summary>
        /// Updates the tip progress value reported by a child package.
        /// Explicit implementation of <see cref="IParent.UpdateTip(double)"/>.
        /// </summary>
        /// <param name="progress">New tip progress value.</param>
        void IParent.UpdateTip(double progress)
        {
            ProgressTip = progress;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Protected dispose pattern implementation.
        /// When <paramref name="disposing"/> is true, disposes managed resources (child package).
        /// </summary>
        /// <param name="disposing">True when called from Dispose; false from finalizer.</param>
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

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed. Automatically provided by the caller.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = default)
        {
            PropertyChanged?.Invoke(
                sender: this,
                e: new PropertyChangedEventArgs(propertyName));
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Raises the <see cref="ProgressCompleted"/> event.
        /// </summary>
        private void OnProgressCompleted()
        {
            ProgressCompleted?.Invoke(
                sender: this,
                e: default);
        }

        /// <summary>
        /// Raises the <see cref="ProgressStarted"/> event.
        /// </summary>
        private void OnProgressRequested()
        {
            ProgressStarted?.Invoke(
                sender: this,
                e: default);
        }

        #endregion Private Methods
    }
}