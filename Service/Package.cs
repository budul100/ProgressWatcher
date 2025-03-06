using System;
using System.Collections.Generic;
using System.Linq;
using ProgressWatcher.Interfaces;

namespace ProgressWatcher
{
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

        internal Package(IParent parent, string status)
        {
            this.parent = parent;
            AllSteps = progressAllSteps;

            progress = new Progress<double>();
            progress.ProgressChanged += OnProgressChanged;

            Progress.Report(0);

            UpdateStatus(status);
        }

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

        public int AllSteps { get; }

        public double CurrentProgress { get; private set; }

        public int CurrentSteps { get; private set; }

        public bool IsCompleted { get; private set; }

        public string Status { get; private set; }

        #endregion Public Properties

        #region Internal Properties

        internal IProgress<double> Progress => progress;

        #endregion Internal Properties

        #region Public Methods

        public void ClearStatus()
        {
            UpdateStatus(default);
        }

        public void Complete()
        {
            child?.Complete();
            UpdateProgress(1);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IParent.DisposeChild()
        {
            child = default;
            CurrentSteps++;

            var currentPogress = CurrentProgress + childWeight;
            UpdateProgress(currentPogress);

            UpdateStatus(Status);
        }

        public IPackage GetPackage()
        {
            return GetPackage(
                steps: 1,
                status: default,
                weight: 0);
        }

        public IPackage GetPackage(string status)
        {
            return GetPackage(
                steps: 1,
                status: status,
                weight: 0);
        }

        public IPackage GetPackage(string status, double weight)
        {
            return GetPackage(
                steps: 1,
                status: status,
                weight: weight);
        }

        public IPackage GetPackage(int steps)
        {
            return GetPackage(
                steps: steps,
                status: default,
                weight: 0);
        }

        public IPackage GetPackage(int steps, string status)
        {
            return GetPackage(
                steps: steps,
                status: status,
                weight: 0);
        }

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

        public IPackage GetPackage<T>(IEnumerable<T> items)
        {
            return GetPackage(
                items: items,
                status: default,
                stepsPerItem: 1,
                weight: 0);
        }

        public IPackage GetPackage<T>(IEnumerable<T> items, string status)
        {
            return GetPackage(
                items: items,
                status: status,
                stepsPerItem: 1,
                weight: 0);
        }

        public IPackage GetPackage<T>(IEnumerable<T> items, int stepsPerItem)
        {
            return GetPackage(
                items: items,
                status: default,
                stepsPerItem: stepsPerItem,
                weight: 0);
        }

        public IPackage GetPackage<T>(IEnumerable<T> items, string status, int stepsPerItem)
        {
            return GetPackage(
                items: items,
                status: status,
                stepsPerItem: stepsPerItem,
                weight: 0);
        }

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

        public IProgress<double> GetProgress()
        {
            return GetProgress(
                status: default,
                weight: default);
        }

        public IProgress<double> GetProgress(string status)
        {
            return GetProgress(
                status: status,
                weight: default);
        }

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

        public void NextStep()
        {
            UpdateSteps(
                steps: 1);
        }

        public void NextStep(int steps)
        {
            UpdateSteps(
                steps: steps);
        }

        public void NextStep(string status)
        {
            NextStep();

            UpdateStatus(status);
        }

        public void NextStep(int steps, string status)
        {
            NextStep(steps);

            UpdateStatus(status);
        }

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

        private double GetChildWeight(double weight)
        {
            var result = weight == 0
                ? (1 - CurrentProgress) / (AllSteps - CurrentSteps)
                : weight;

            return result;
        }

        private void OnProgressChanged(object sender, double progress)
        {
            CurrentSteps = Convert.ToInt32(Math.Floor(progress * AllSteps));

            UpdateProgress(progress);
        }

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

        private void UpdateStatus(string status)
        {
            Status = status;

            parent.UpdateTip(status);
        }

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