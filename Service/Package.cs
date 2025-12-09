using System;
using System.Collections.Generic;
using System.Linq;
using ProgressWatcher.Interfaces;

namespace ProgressWatcher
{
    /// <summary>
    /// Represents a progress package that can contain child packages and report progress to a parent.
    /// </summary>
    public class Package
        : IPackage, IParent
    {
        #region Private Fields

        private const int progressAllSteps = 1000;

        private readonly IParent parent;
        private readonly Progress<double> progress;

        private Package child;
        private double childWeight;
        private bool isDisposed;

        #endregion Private Fields

        #region Internal Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Package"/> class with the default number of steps.
        /// </summary>
        /// <param name="parent">The parent that receives progress updates.</param>
        /// <param name="status">Initial status message for the package.</param>
        internal Package(IParent parent, string status)
        {
            this.parent = parent;
            AllSteps = progressAllSteps;

            progress = new Progress<double>();
            progress.ProgressChanged += OnProgressChanged;

            Progress.Report(0);

            UpdateStatus(status);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Package"/> class with a specific number of steps.
        /// </summary>
        /// <param name="parent">The parent that receives progress updates.</param>
        /// <param name="status">Initial status message for the package.</param>
        /// <param name="steps">Total steps this package represents. Must be >= 0.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="steps"/> is less than 0.</exception>
        internal Package(IParent parent, string status, int steps)
        {
            if (steps < 0)
            {
                throw new ArgumentException(
                    message: "The package cannot have less than 0 steps.",
                    paramName: nameof(steps));
            }

            this.parent = parent;
            AllSteps = steps;

            UpdateStatus(status);
        }

        #endregion Internal Constructors

        #region Public Properties

        /// <summary>
        /// Gets the total number of steps for this package.
        /// </summary>
        public int AllSteps { get; }

        /// <summary>
        /// Gets the current progress as a value between 0.0 and 1.0.
        /// </summary>
        public double CurrentProgress { get; private set; }

        /// <summary>
        /// Gets the number of completed steps.
        /// </summary>
        public int CurrentSteps { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the package has completed (progress >= 1.0).
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Gets the current status message associated with this package.
        /// </summary>
        public string Status { get; private set; }

        #endregion Public Properties

        #region Internal Properties

        /// <summary>
        /// Gets the internal <see cref="IProgress{Double}"/> used to report fractional progress for this package.
        /// </summary>
        internal IProgress<double> Progress => progress;

        #endregion Internal Properties

        #region Public Methods

        /// <summary>
        /// Clears the current status (sets it to null).
        /// </summary>
        public void ClearStatus()
        {
            UpdateStatus(default);
        }

        /// <summary>
        /// Marks this package and any active child packages as complete.
        /// </summary>
        public void Complete()
        {
            child?.Complete();
            UpdateProgress(1);
        }

        /// <summary>
        /// Releases resources used by the package and notifies the parent.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current child package and advances the step counter.
        /// </summary>
        void IParent.DisposeChild()
        {
            child = default;
            CurrentSteps++;

            var currentPogress = CurrentProgress + childWeight;
            UpdateProgress(currentPogress);

            UpdateStatus(Status);
        }

        /// <summary>
        /// Creates and returns a child package with a single step.
        /// </summary>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage()
        {
            return GetPackage(
                steps: 1,
                status: default,
                weight: 0);
        }

        /// <summary>
        /// Creates and returns a child package with a single step and an initial status.
        /// </summary>
        /// <param name="status">Initial status for the child package.</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage(string status)
        {
            return GetPackage(
                steps: 1,
                status: status,
                weight: 0);
        }

        /// <summary>
        /// Creates and returns a child package with a single step, initial status and a weight.
        /// </summary>
        /// <param name="status">Initial status for the child package.</param>
        /// <param name="weight">Weight of the child within the parent's remaining progress (0.0 - 1.0).</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="weight"/> is outside the range [0,1].</exception>
        public IPackage GetPackage(string status, double weight)
        {
            return GetPackage(
                steps: 1,
                status: status,
                weight: weight);
        }

        /// <summary>
        /// Creates and returns a child package with a specific number of steps.
        /// </summary>
        /// <param name="steps">Number of steps for the child package.</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage(int steps)
        {
            return GetPackage(
                steps: steps,
                status: default,
                weight: 0);
        }

        /// <summary>
        /// Creates and returns a child package with a specific number of steps and an initial status.
        /// </summary>
        /// <param name="steps">Number of steps for the child package.</param>
        /// <param name="status">Initial status for the child package.</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage(int steps, string status)
        {
            return GetPackage(
                steps: steps,
                status: status,
                weight: 0);
        }

        /// <summary>
        /// Creates and returns a child package with steps, status and weight.
        /// </summary>
        /// <param name="steps">Number of steps for the child package.</param>
        /// <param name="status">Initial status for the child package.</param>
        /// <param name="weight">Weight of the child within the parent's remaining progress (0.0 - 1.0).</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="weight"/> is outside the range [0,1].</exception>
        public IPackage GetPackage(int steps, string status, double weight)
        {
            if (weight < 0 || weight > 1)
            {
                throw new ArgumentException(
                    message: "The weight of a package must be greater than 0 and less or equal to 1.",
                    paramName: nameof(weight));
            }

            CreateChild(
                status: status,
                weight: weight,
                steps: steps,
                isProgress: false);

            return child;
        }

        /// <summary>
        /// Creates a child package that will represent processing of the given collection using one step per item.
        /// </summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Items to be processed.</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage<T>(IEnumerable<T> items)
        {
            return GetPackage(
                items: items,
                status: default,
                stepsPerItem: 1,
                weight: 0);
        }

        /// <summary>
        /// Creates a child package for the given collection with an initial status, using one step per item.
        /// </summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Items to be processed.</param>
        /// <param name="status">Initial status for the child package.</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage<T>(IEnumerable<T> items, string status)
        {
            return GetPackage(
                items: items,
                status: status,
                stepsPerItem: 1,
                weight: 0);
        }

        /// <summary>
        /// Creates a child package for the given collection with a specified number of steps per item.
        /// </summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Items to be processed.</param>
        /// <param name="stepsPerItem">Number of steps to allocate per item (must be > 0).</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage<T>(IEnumerable<T> items, int stepsPerItem)
        {
            return GetPackage(
                items: items,
                status: default,
                stepsPerItem: stepsPerItem,
                weight: 0);
        }

        /// <summary>
        /// Creates a child package for the given collection with status and steps-per-item.
        /// </summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Items to be processed.</param>
        /// <param name="status">Initial status for the child package.</param>
        /// <param name="stepsPerItem">Number of steps to allocate per item (must be > 0).</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        public IPackage GetPackage<T>(IEnumerable<T> items, string status, int stepsPerItem)
        {
            return GetPackage(
                items: items,
                status: status,
                stepsPerItem: stepsPerItem,
                weight: 0);
        }

        /// <summary>
        /// Creates a child package representing processing of a collection with steps-per-item and weight.
        /// </summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Items to be processed.</param>
        /// <param name="status">Initial status for the child package.</param>
        /// <param name="stepsPerItem">Number of steps to allocate per item (must be > 0).</param>
        /// <param name="weight">Weight of the child within the parent's remaining progress (0.0 - 1.0).</param>
        /// <returns>A child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stepsPerItem"/> less than 0 or <paramref name="weight"/> is outside [0,1].</exception>
        public IPackage GetPackage<T>(IEnumerable<T> items, string status, int stepsPerItem, double weight)
        {
            if (stepsPerItem <= 0)
            {
                throw new ArgumentException(
                    message: "The steps per progress item must be greater than 0.",
                    paramName: nameof(stepsPerItem));
            }

            if (weight < 0 || weight > 1)
            {
                throw new ArgumentException(
                    message: "The weight of a package must be greater than 0 and less or equal to 1.",
                    paramName: nameof(weight));
            }

            CreateChild(
                status: status,
                weight: weight,
                steps: (items?.Count() ?? 0) * stepsPerItem,
                isProgress: false);

            return child;
        }

        /// <summary>
        /// Creates and returns a child progress reporter without status or weight.
        /// </summary>
        /// <returns>An <see cref="IProgress{Double}"/> instance reporting child progress.</returns>
        public IProgress<double> GetProgress()
        {
            return GetProgress(
                status: default,
                weight: default);
        }

        /// <summary>
        /// Creates and returns a child progress reporter with an initial status.
        /// </summary>
        /// <param name="status">Initial status for the progress reporter.</param>
        /// <returns>An <see cref="IProgress{Double}"/> instance reporting child progress.</returns>
        public IProgress<double> GetProgress(string status)
        {
            return GetProgress(
                status: status,
                weight: default);
        }

        /// <summary>
        /// Creates and returns a child progress reporter with initial status and weight.
        /// </summary>
        /// <param name="status">Initial status for the progress reporter.</param>
        /// <param name="weight">Weight of the reporter within the parent's remaining progress (0.0 - 1.0).</param>
        /// <returns>An <see cref="IProgress{Double}"/> instance reporting child progress.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="weight"/> is outside the range [0,1].</exception>
        public IProgress<double> GetProgress(string status, double weight)
        {
            if (weight < 0 || weight > 1)
            {
                throw new ArgumentException(
                    message: "The weight of a package must be greater than 0 and less or equal to 1.",
                    paramName: nameof(weight));
            }

            CreateChild(
                status: status,
                weight: weight,
                steps: 0,
                isProgress: true);

            return child.Progress;
        }

        /// <summary>
        /// Advances the progress by one step.
        /// </summary>
        public void NextStep()
        {
            UpdateSteps(
                steps: 1);
        }

        /// <summary>
        /// Advances the progress by the specified number of steps.
        /// </summary>
        /// <param name="steps">Number of steps to advance.</param>
        public void NextStep(int steps)
        {
            UpdateSteps(
                steps: steps);
        }

        /// <summary>
        /// Advances the progress by one step and updates the status.
        /// </summary>
        /// <param name="status">New status message.</param>
        public void NextStep(string status)
        {
            NextStep();

            UpdateStatus(status);
        }

        /// <summary>
        /// Advances the progress by the specified number of steps and updates the status.
        /// </summary>
        /// <param name="steps">Number of steps to advance.</param>
        /// <param name="status">New status message.</param>
        public void NextStep(int steps, string status)
        {
            NextStep(steps);

            UpdateStatus(status);
        }

        /// <summary>
        /// Sets the package status message.
        /// </summary>
        /// <param name="status">New status message or null to clear.</param>
        public void SetStatus(string status)
        {
            UpdateStatus(status);
        }

        void IParent.StartChild()
        {
            parent.StartChild();
        }

        void IParent.UpdateChild(double progress)
        {
            var currentProgress = CurrentProgress + (progress * childWeight);
            parent.UpdateChild(currentProgress);
        }

        void IParent.UpdateTip(double progress)
        {
            parent.UpdateTip(progress);
        }

        void IParent.UpdateTip(string status)
        {
            parent.UpdateTip(status);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Releases resources used by the package.
        /// </summary>
        /// <param name="disposing">True when called from Dispose; false when called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Complete();
                    parent?.DisposeChild();
                }

                isDisposed = true;
            }
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Completes and disposes the current child package if present.
        /// </summary>
        private void CompleteChild()
        {
            if (child != default)
            {
                if (!child.IsCompleted
                    && child.Progress == default)
                {
                    throw new InvalidOperationException(
                        "There is still an uncompleted progress child package.");
                }

                child.Dispose();
            }
        }

        /// <summary>
        /// Creates a child package or child progress reporter.
        /// </summary>
        /// <param name="status">Initial status for the child.</param>
        /// <param name="weight">Weight of the child within the parent's remaining progress.</param>
        /// <param name="steps">Number of steps for the child (0 for progress-only children).</param>
        /// <param name="isProgress">Indicates whether this child is a progress reporter (true) or a step-based package (false).</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="weight"/> is outside [0,1] or resulting progress would exceed 1.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the parent package is already completed or has no child slots left.</exception>
        private void CreateChild(string status, double weight, int steps, bool isProgress)
        {
            if (weight < 0 || weight > 1)
            {
                throw new ArgumentException(
                    message: "The weight of a package must be between 0 and 1.",
                    paramName: nameof(weight));
            }

            if (CurrentProgress + weight > 1)
            {
                throw new ArgumentException(
                    message: $"The weight cannot be greater than the resulting progress of {1 - CurrentProgress}.",
                    paramName: nameof(weight));
            }

            if (IsCompleted)
            {
                throw new InvalidOperationException(
                    message: "The package is already completed so a new progress child package cannot be created.");
            }

            if (CurrentSteps >= AllSteps)
            {
                throw new InvalidOperationException(
                    message: "There are no childs left to create a new progress child package.");
            }

            CompleteChild();

            status = string.IsNullOrWhiteSpace(status)
                ? Status
                : status;

            if (isProgress)
            {
                child = new Package(
                    parent: this,
                    status: status);
            }
            else
            {
                child = new Package(
                    parent: this,
                    status: status,
                    steps: steps);
            }

            childWeight = GetChildWeight(weight);

            parent.StartChild();
        }

        /// <summary>
        /// Calculates the effective weight for a child when no explicit weight was provided.
        /// </summary>
        /// <param name="weight">Provided weight (0 means automatic calculation).</param>
        /// <returns>Computed weight between 0 and 1.</returns>
        private double GetChildWeight(double weight)
        {
            var result = weight == 0
                ? (1 - CurrentProgress) / (AllSteps - CurrentSteps)
                : weight;

            return result;
        }

        /// <summary>
        /// Handler for internal progress changes; updates current steps and overall progress.
        /// </summary>
        /// <param name="sender">Sender of the progress event.</param>
        /// <param name="progress">Progress value between 0.0 and 1.0.</param>
        private void OnProgressChanged(object sender, double progress)
        {
            CurrentSteps = Convert.ToInt32(Math.Floor(progress * AllSteps));

            UpdateProgress(progress);
        }

        /// <summary>
        /// Updates internal progress state and notifies the parent when progress advances.
        /// </summary>
        /// <param name="progress">New progress value.</param>
        private void UpdateProgress(double progress)
        {
            if (progress > 1)
            {
                progress = 1;
            }

            if (progress > CurrentProgress)
            {
                CurrentProgress = progress;
                IsCompleted = progress >= 1;

                parent.UpdateTip(CurrentProgress);
                parent.UpdateChild(CurrentProgress);
            }
        }

        /// <summary>
        /// Updates the current status message and notifies the parent.
        /// </summary>
        /// <param name="status">New status message.</param>
        private void UpdateStatus(string status)
        {
            Status = status;

            parent.UpdateTip(status);
        }

        /// <summary>
        /// Advances internal step count and updates progress accordingly.
        /// </summary>
        /// <param name="steps">Number of steps to advance (must be >= 0).</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="steps"/> is less than 0.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the package is disposed or already completed.</exception>
        private void UpdateSteps(int steps)
        {
            if (steps < 0)
            {
                throw new ArgumentException(
                    message: "The progress cannot have less than 0 steps.",
                    paramName: nameof(steps));
            }

            if (isDisposed || parent == default)
            {
                throw new InvalidOperationException(
                    message: "The progress package is already disposed.");
            }

            if (IsCompleted)
            {
                throw new InvalidOperationException(
                    message: "The progress package is already completed.");
            }

            CompleteChild();

            CurrentSteps += steps;

            if (CurrentSteps > AllSteps)
            {
                CurrentSteps = AllSteps;
            }

            var currentProgress = (double)CurrentSteps / AllSteps;

            UpdateProgress(currentProgress);
        }

        #endregion Private Methods
    }
}