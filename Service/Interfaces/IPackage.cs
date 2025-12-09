using System;
using System.Collections.Generic;

namespace ProgressWatcher.Interfaces
{
    /// <summary>
    /// Represents a progress package that can contain child packages, report progress and status,
    /// and be used either as a step-based package or as a progress reporter.
    /// </summary>
    public interface IPackage
        : IDisposable
    {
        #region Public Properties

        /// <summary>
        /// Gets the total number of steps that this package represents.
        /// For progress-only packages this value may represent a default or logical step count.
        /// </summary>
        int AllSteps { get; }

        /// <summary>
        /// Gets the current progress value in the range [0.0, 1.0].
        /// </summary>
        double CurrentProgress { get; }

        /// <summary>
        /// Gets the number of completed steps.
        /// </summary>
        int CurrentSteps { get; }

        /// <summary>
        /// Gets a value indicating whether the package is completed (progress >= 1.0).
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets the current status message associated with this package.
        /// </summary>
        string Status { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Clears the current status message (sets it to null).
        /// </summary>
        void ClearStatus();

        /// <summary>
        /// Marks the package and any active child packages as complete.
        /// </summary>
        void Complete();

        /// <summary>
        /// Creates a child package to represent processing of the provided collection using one step per item.
        /// </summary>
        /// <typeparam name="T">The item type in the collection.</typeparam>
        /// <param name="items">The collection of items to be processed.</param>
        /// <returns>A new child <see cref="IPackage"/> that represents the work for the given items.</returns>
        IPackage GetPackage<T>(IEnumerable<T> items);

        /// <summary>
        /// Creates a child package for the provided collection with an initial status and one step per item.
        /// </summary>
        /// <typeparam name="T">The item type in the collection.</typeparam>
        /// <param name="items">The collection of items to be processed.</param>
        /// <param name="status">Initial status message for the child package.</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        IPackage GetPackage<T>(IEnumerable<T> items, string status);

        /// <summary>
        /// Creates a child package for the provided collection with a specified number of steps per item.
        /// </summary>
        /// <typeparam name="T">The item type in the collection.</typeparam>
        /// <param name="items">The collection of items to be processed.</param>
        /// <param name="stepsPerItem">Number of steps to allocate per item (must be greater than 0).</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stepsPerItem"/> is less than or equal to 0.</exception>
        IPackage GetPackage<T>(IEnumerable<T> items, int stepsPerItem);

        /// <summary>
        /// Creates a child package for the provided collection with status and a specified number of steps per item.
        /// </summary>
        /// <typeparam name="T">The item type in the collection.</typeparam>
        /// <param name="items">The collection of items to be processed.</param>
        /// <param name="status">Initial status message for the child package.</param>
        /// <param name="stepsPerItem">Number of steps to allocate per item (must be greater than 0).</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stepsPerItem"/> is less than or equal to 0.</exception>
        IPackage GetPackage<T>(IEnumerable<T> items, string status, int stepsPerItem);

        /// <summary>
        /// Creates a child package for the provided collection with steps-per-item and an explicit weight.
        /// </summary>
        /// <typeparam name="T">The item type in the collection.</typeparam>
        /// <param name="items">The collection of items to be processed.</param>
        /// <param name="status">Initial status message for the child package.</param>
        /// <param name="stepsPerItem">Number of steps to allocate per item (must be greater than 0).</param>
        /// <param name="weight">Relative weight of the child within the parent's remaining progress (range [0.0, 1.0]).</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stepsPerItem"/> is less than or equal to 0, or when <paramref name="weight"/> is outside [0,1].</exception>
        IPackage GetPackage<T>(IEnumerable<T> items, string status, int stepsPerItem, double weight);

        /// <summary>
        /// Creates a child package with the specified number of steps.
        /// </summary>
        /// <param name="steps">Total steps for the child package.</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        IPackage GetPackage(int steps);

        /// <summary>
        /// Creates a child package with the specified number of steps and an initial status.
        /// </summary>
        /// <param name="steps">Total steps for the child package.</param>
        /// <param name="status">Initial status message for the child package.</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        IPackage GetPackage(int steps, string status);

        /// <summary>
        /// Creates a child package with steps, status and an explicit weight.
        /// </summary>
        /// <param name="steps">Total steps for the child package.</param>
        /// <param name="status">Initial status message for the child package.</param>
        /// <param name="weight">Relative weight of the child within the parent's remaining progress (range [0.0, 1.0]).</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="weight"/> is outside the range [0,1].</exception>
        IPackage GetPackage(int steps, string status, double weight);

        /// <summary>
        /// Creates and returns a child package with a single step.
        /// </summary>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        IPackage GetPackage();

        /// <summary>
        /// Creates and returns a child package with a single step and an initial status.
        /// </summary>
        /// <param name="status">Initial status message for the child package.</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        IPackage GetPackage(string status);

        /// <summary>
        /// Creates and returns a child package with a single step, initial status and an explicit weight.
        /// </summary>
        /// <param name="status">Initial status message for the child package.</param>
        /// <param name="weight">Relative weight of the child within the parent's remaining progress (range [0.0, 1.0]).</param>
        /// <returns>A new child <see cref="IPackage"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="weight"/> is outside the range [0,1].</exception>
        IPackage GetPackage(string status, double weight);

        /// <summary>
        /// Creates and returns an <see cref="IProgress{Double}"/> instance that reports progress for a child
        /// progress reporter (progress-only child).
        /// </summary>
        /// <returns>An <see cref="IProgress{Double}"/> used to report child progress values in [0.0,1.0].</returns>
        IProgress<double> GetProgress();

        /// <summary>
        /// Creates and returns an <see cref="IProgress{Double}"/> instance with an initial status.
        /// </summary>
        /// <param name="status">Initial status message for the progress reporter.</param>
        /// <returns>An <see cref="IProgress{Double}"/> used to report child progress values in [0.0,1.0].</returns>
        IProgress<double> GetProgress(string status);

        /// <summary>
        /// Creates and returns an <see cref="IProgress{Double}"/> instance with initial status and explicit weight.
        /// </summary>
        /// <param name="status">Initial status message for the progress reporter.</param>
        /// <param name="weight">Relative weight of the reporter within the parent's remaining progress (range [0.0, 1.0]).</param>
        /// <returns>An <see cref="IProgress{Double}"/> used to report child progress values in [0.0,1.0].</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="weight"/> is outside the range [0,1].</exception>
        IProgress<double> GetProgress(string status, double weight);

        /// <summary>
        /// Advances the package by one step.
        /// </summary>
        void NextStep();

        /// <summary>
        /// Advances the package by the specified number of steps.
        /// </summary>
        /// <param name="steps">Number of steps to advance (must be >= 0).</param>
        void NextStep(int steps);

        /// <summary>
        /// Advances the package by the specified number of steps and updates the status.
        /// </summary>
        /// <param name="steps">Number of steps to advance (must be >= 0).</param>
        /// <param name="status">New status message after advancing.</param>
        void NextStep(int steps, string status);

        /// <summary>
        /// Advances the package by one step and updates the status.
        /// </summary>
        /// <param name="status">New status message after advancing.</param>
        void NextStep(string status);

        /// <summary>
        /// Sets or updates the current status message for this package.
        /// </summary>
        /// <param name="status">New status message or null to clear.</param>
        void SetStatus(string status);

        #endregion Public Methods
    }
}