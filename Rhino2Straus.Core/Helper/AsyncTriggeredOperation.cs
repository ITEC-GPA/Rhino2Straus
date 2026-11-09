#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rhino2Straus.Core.Helper;

/// <summary>Pollable, rising-edge operation. Callers must capture immutable inputs in work.</summary>
public sealed class AsyncTriggeredOperation<T> : IDisposable where T : class
{
    private readonly object gate = new();
    private readonly Func<Func<CancellationToken, Task<T>>, CancellationToken, Task<T>> executor;
    private string? previousKey;
    private bool previousRun, disposed;
    private long generation, revision;
    private T? value;
    private string status = "Run required";
    private string? error;
    private Execution? active;

    private sealed class Execution(long generation)
    {
        public readonly long Generation = generation;
        public readonly CancellationTokenSource Source = new();
        public Task Cancellation = Task.CompletedTask;
    }

    public AsyncTriggeredOperation(Func<Func<CancellationToken, T>, CancellationToken, Task<T>> executor)
    {
        ArgumentNullException.ThrowIfNull(executor);
        this.executor = (work, token) => executor(ct => work(ct).GetAwaiter().GetResult(), token);
    }

    private AsyncTriggeredOperation(Func<Func<CancellationToken, Task<T>>, CancellationToken, Task<T>> executor, bool asyncWork)
        => this.executor = executor ?? throw new ArgumentNullException(nameof(executor));

    public static AsyncTriggeredOperation<T> CreateAsync(Func<Func<CancellationToken, Task<T>>, CancellationToken, Task<T>> executor)
        => new(executor, true);

    public T? Value { get { lock (gate) return value; } }
    public string Status { get { lock (gate) return status; } }
    public string? Error { get { lock (gate) return error; } }
    public long Revision { get { lock (gate) return revision; } }
    public bool IsPending { get { lock (gate) return active != null; } }

    /// <summary>Never waits for execution. A held-high run signal does not restart changed inputs.</summary>
    public T? Update(string key, bool run, Func<CancellationToken, T> work, Func<T, string> completed)
        => UpdatePrepared(key, run, () => work, completed);

    /// <summary>Captures the snapshot on the calling thread, only when a new job is accepted.</summary>
    public T? UpdatePrepared(string key, bool run, Func<Func<CancellationToken, T>> prepareWork, Func<T, string> completed)

        => UpdatePreparedAsync(key, run, () => { var work = prepareWork(); return ct => Task.FromResult(work(ct)); }, completed);

    public T? UpdatePreparedAsync(string key, bool run, Func<Func<CancellationToken, Task<T>>> prepareWork, Func<T, string> completed)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(prepareWork);
        ArgumentNullException.ThrowIfNull(completed);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            bool rising = run && !previousRun;
            previousRun = run;
            if (!string.Equals(key, previousKey, StringComparison.Ordinal))
            {
                previousKey = key;
                Discard();
            }
            if (rising && active == null)
            {
                value = null;
                error = null;
                Func<CancellationToken, Task<T>> work;
                try { work = prepareWork() ?? throw new InvalidOperationException("Missing analysis work."); }
                catch (Exception exception)
                {
                    status = "Failed"; error = exception.Message; revision++;
                    return null;
                }
                status = "Queued";
                var execution = new Execution(++generation);
                active = execution;
                revision++;
                // Even an executor that blocks or throws synchronously cannot block Update.
                _ = Task.Run(() => ExecuteAsync(execution, work, completed));
            }
            return value;
        }
    }

    /// <summary>Lifecycle reset must observe false again before accepting a new Run.</summary>
    public void RequireRunReset()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            Discard();
            previousRun = true;
        }
    }

    /// <summary>Discards output without resetting the run edge; false then true rearms it.</summary>
    public void Invalidate()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            Discard();
        }
    }

    private void Discard()
    {
        generation++;
        if (active != null)
        {
            // Sets cancellation immediately, but runs arbitrary token callbacks asynchronously.
            active.Cancellation = active.Source.CancelAsync();
            active = null;
        }
        value = null;
        error = null;
        status = "Run required";
        revision++;
    }

    private bool IsCurrent(Execution execution) =>
        !disposed && generation == execution.Generation && ReferenceEquals(active, execution);

    private async Task ExecuteAsync(Execution execution, Func<CancellationToken, Task<T>> work, Func<T, string> completed)
    {
        try
        {
            var token = execution.Source.Token;
            token.ThrowIfCancellationRequested();
            var result = await executor(ct =>
            {
                // An executor may queue work without honoring its token itself.
                ct.ThrowIfCancellationRequested();
                token.ThrowIfCancellationRequested();
                lock (gate)
                {
                    if (!IsCurrent(execution)) throw new OperationCanceledException(token);
                    status = "Running";
                    revision++;
                }
                return work(ct);
            }, token).ConfigureAwait(false);
            lock (gate)
                if (!IsCurrent(execution)) return;

            string resultStatus = completed(result);
            lock (gate)
            {
                if (!IsCurrent(execution)) return;
                value = result;
                status = resultStatus;
                active = null;
                revision++;
            }
        }
        catch (Exception exception)
        {
            lock (gate)
            {
                if (IsCurrent(execution))
                {
                    value = null;
                    error = exception.Message;
                    status = exception is OperationCanceledException ? "Cancelled" : "Failed";
                    active = null;
                    revision++;
                }
            }
        }
        finally
        {
            Task cancellation;
            lock (gate) cancellation = execution.Cancellation;
            // Observe callback failures too, and do not dispose while callbacks are still running.
            try { await cancellation.ConfigureAwait(false); }
            catch (Exception) { /* Obsolete cancellation must not overwrite current state. */ }
            execution.Source.Dispose();
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            Discard();
            disposed = true;
            status = "Disposed";
        }
    }
}
