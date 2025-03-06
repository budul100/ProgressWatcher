using System;
using System.Collections.Generic;

namespace ProgressWatcher.Interfaces
{
    public interface IPackage
        : IDisposable
    {
        #region Public Properties

        int AllSteps { get; }

        double CurrentProgress { get; }

        int CurrentSteps { get; }

        bool IsCompleted { get; }

        string Status { get; }

        #endregion Public Properties

        #region Public Methods

        void ClearStatus();

        void Complete();

        IPackage GetPackage<T>(IEnumerable<T> items);

        IPackage GetPackage<T>(IEnumerable<T> items, string status);

        IPackage GetPackage<T>(IEnumerable<T> items, int stepsPerItem);

        IPackage GetPackage<T>(IEnumerable<T> items, string status, int stepsPerItem);

        IPackage GetPackage<T>(IEnumerable<T> items, string status, int stepsPerItem, double weight);

        IPackage GetPackage(int steps);

        IPackage GetPackage(int steps, string status);

        IPackage GetPackage(int steps, string status, double weight);

        IPackage GetPackage();

        IPackage GetPackage(string status);

        IPackage GetPackage(string status, double weight);

        IProgress<double> GetProgress();

        IProgress<double> GetProgress(string status);

        IProgress<double> GetProgress(string status, double weight);

        void NextStep();

        void NextStep(int steps);

        void NextStep(int steps, string status);

        void NextStep(string status);

        void SetStatus(string status);

        #endregion Public Methods
    }
}