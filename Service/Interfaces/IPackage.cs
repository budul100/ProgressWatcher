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

        IPackage GetPackage<T>(IEnumerable<T> items, int stepsPerItem = 1, string status = default);

        IPackage GetPackage<T>(IEnumerable<T> items, int stepsPerItem, string status, double weight);

        IPackage GetPackage(int steps, string status = default);

        IPackage GetPackage(int steps, string status, double weight);

        IPackage GetPackage(string status = default);

        IPackage GetPackage(string status, double weight);

        IProgress<double> GetProgress(string status = default);

        IProgress<double> GetProgress(string status, double weight);

        void NextStep(int steps = 1);

        void NextStep(int steps, string status);

        void NextStep(string status);

        void SetStatus(string status);

        #endregion Public Methods
    }
}