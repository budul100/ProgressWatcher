# ProgressWatcher

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/budul100/ProgressWatcher/blob/main/LICENSE) [![NuGet](https://img.shields.io/nuget/v/budul.ProgressWatcher.svg)](https://www.nuget.org/packages/budul.ProgressWatcher/)

Lightweight, thread-safe progress coordination library for nested and parallel operations.

- PackageId: `budul.ProgressWatcher`
- Supported TFMs: `net8.0`, `net9.0`, `net10.0`
- Repository: https://github.com/budul100/ProgressWatcher
- License: MIT

## Overview

`ProgressWatcher` provides a small API to create progress packages (`IPackage`) and a top-level `Watcher` that aggregates progress and status information for UI or logging consumers. Packages support nested child packages, fractional progress reporters (`IProgress<double>`), and efficient thread-local batching for high-concurrency scenarios.

## Installation

Install from NuGet:

```dotnet add package budul.ProgressWatcher```

## Quick start (synchronous)

```cs
using ProgressWatcher;

var watcher = new Watcher(); 
var package = watcher.GetPackage(allSteps: 3, status: "Starting work...");

package.NextStep("Step 1 done"); 
package.NextStep("Step 2 done"); 
package.NextStep("Step 3 done");
package.Dispose(); // or package.Complete() 

// watcher.ProgressAll, watcher.ProgressTip and watcher.Status update via PropertyChanged
```

## Parallel usage (hot loops)

For high-concurrency loops (e.g. `Parallel.ForEach` or many threads) use `NextStepLocal()` to avoid contention. After the loop finishes, call `FlushCounterLocal()` to ensure any thread-local counts are aggregated and reported.

```cs
using ProgressWatcher; 
using System.Threading.Tasks;

var items = Enumerable.Range(0, 1_000_000); 

var watcher = new Watcher(); 
var package = watcher.GetPackage(items, "Processing items");

Parallel.ForEach(items, item => { // work on item...
	// report work from current thread using thread-local batching
	package.NextStepLocal();
});

// ensure remaining local counts are flushed 
package.FlushCounterLocal(); 
package.Dispose();
```

Important notes:
- Always call `FlushCounterLocal()` after a parallel loop; otherwise some thread-local counts may remain unreported.
- `NextStepLocal()` is intended for hot/tight loops and batches updates on a per-thread basis to reduce synchronization costs.

## Using progress reporters and weights

Get a fractional progress reporter when you need to report continuous progress from a subtask:

```cs
var parent = watcher.GetPackage(status: "Parent", steps: 1); 

using var reporter = parent.GetProgress(status: "Loading", steps: 10, weight: 0.5)
{
	reporter.NextStep(4);
}

parent.Dispose();
```

Weights:
- `weight` is a value in [0.0, 1.0] that assigns how much of the parent's remaining progress the child consumes.
- Passing `0` uses automatic weight calculation based on remaining steps.

## Nesting and lifecycle

- Create nested work with `GetPackage(...)` or fractional reporters with `GetProgress(...)`.
- Call `Dispose()` on a package to complete it and notify the parent. `Complete()` sets progress to 100% immediately.
- Top-level `Watcher` exposes events: `ProgressStarted`, `ProgressCompleted`, `ChildStarted`, `ChildDisposed` and properties `ProgressAll`, `ProgressTip`, `Status`, `IsRunning` (useful for UI binding).

## Thread-safety and throttling

- `Package` is designed for concurrent reporting. Use `NextStepLocal()` in parallel scenarios and `FlushCounterLocal()` afterwards.
- Progress updates are internally throttled using a minimum delta and minimum interval before forwarding to the parent. This avoids flooding UI updates while remaining responsive.

## Best practices

- Prefer `NextStepLocal()` inside parallel loops; call `FlushCounterLocal()` after the loop.
- Dispose or `Complete()` packages to ensure parents advance to completion.
- Use `GetProgress(...)` for long-running subtasks that report fractional progress.

## API summary

- `Watcher` — top-level aggregator; use `GetPackage(...)` to start work and bind to `ProgressAll`, `ProgressTip`, `Status`.
- `IPackage` — use `NextStep()`, `NextStepLocal()`, `GetPackage(...)`, `GetProgress(...)`, `FlushCounterLocal()`, `Complete()`, `Dispose()`.