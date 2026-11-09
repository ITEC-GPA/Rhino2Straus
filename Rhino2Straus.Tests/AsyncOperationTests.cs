#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Rhino2Straus.Core.Helper;

internal static class AsyncOperationTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
    private static void Wait(ManualResetEventSlim signal)
    {
        if (!signal.Wait(Timeout)) throw new TimeoutException("Test signal was not received.");
    }
    private static void Until(Func<bool> condition)
    {
        if (!SpinWait.SpinUntil(condition, Timeout)) throw new TimeoutException("State did not settle.");
    }
    private static Task<string> Execute(Func<CancellationToken, string> work, CancellationToken token) =>
        Task.Run(() => work(token), token);
    private static string Completed(string value) => "Done: " + value;

    public static void Run(Action<string, Action> test, Action<bool, string> assert)
    {
        test("Async update is nonblocking, single-edge and rearmable", () =>
        {
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var operation = new AsyncTriggeredOperation<string>(Execute);
            int calls = 0;
            string Work(CancellationToken token)
            {
                Interlocked.Increment(ref calls);
                entered.Set();
                Wait(release);
                return "result";
            }
            try
            {
                var update = Task.Run(() => operation.Update("A", true, Work, Completed));
                assert(update.Wait(Timeout), "Update blocked on work.");
                assert(update.Result == null, "Pending update returned output.");
                Wait(entered);
                long revision = operation.Revision;
                operation.Update("A", true, Work, Completed);
                operation.Update("A", false, Work, Completed);
                operation.Update("A", true, Work, Completed);
                assert(calls == 1 && operation.IsPending, "Duplicate active execution.");
                assert(operation.Revision == revision, "Polling changed revision.");
                release.Set();
                Until(() => !operation.IsPending);
                assert(operation.Value == "result" && operation.Status == "Done: result", "Missing result.");
                assert(operation.Revision > revision, "Completion did not change revision.");
                assert(operation.Update("A", true, Work, Completed) == "result" && calls == 1, "Held-high reran.");
                operation.Update("A", false, Work, Completed);
                operation.Update("A", true, Work, Completed);
                Until(() => !operation.IsPending);
                assert(calls == 2, "New rising edge did not run.");
            }
            finally { release.Set(); }
        });

        test("Async key change and ABA suppress late native results", () =>
        {
            using var oldEntered = new ManualResetEventSlim();
            using var releaseOld = new ManualResetEventSlim();
            using var oldFinished = new ManualResetEventSlim();
            using var newEntered = new ManualResetEventSlim();
            using var releaseNew = new ManualResetEventSlim();
            using var operation = new AsyncTriggeredOperation<string>(Execute);
            CancellationToken oldToken = default;
            int oldFormatted = 0;
            try
            {
                operation.Update("A", true, token =>
                {
                    oldToken = token;
                    oldEntered.Set();
                    Wait(releaseOld);
                    oldFinished.Set();
                    return "old"; // Deliberately ignores cancellation, like in-flight native work.
                }, value => { Interlocked.Increment(ref oldFormatted); return value; });
                Wait(oldEntered);
                operation.Update("B", true, _ => "unused", Completed);
                assert(oldToken.IsCancellationRequested && !operation.IsPending && operation.Value == null,
                    "Changed key did not cancel/discard, or held-high started work.");
                operation.Update("B", false, _ => "unused", Completed);
                operation.Update("A", true, _ =>
                {
                    newEntered.Set();
                    Wait(releaseNew);
                    return "new";
                }, Completed);
                Wait(newEntered);
                releaseOld.Set();
                Wait(oldFinished);
                releaseNew.Set();
                Until(() => !operation.IsPending);
                assert(operation.Value == "new" && oldFormatted == 0, "ABA accepted stale output.");
            }
            finally { releaseOld.Set(); releaseNew.Set(); }
        });

        test("Async failures are sticky until rising edge or changed key", () =>
        {
            using var operation = new AsyncTriggeredOperation<string>(Execute);
            operation.Update("A", true, _ => throw new InvalidOperationException("work failed"), Completed);
            Until(() => !operation.IsPending);
            assert(operation.Status == "Failed" && operation.Error == "work failed", "Work failure lost.");
            operation.Update("A", true, _ => "ok", Completed);
            operation.Update("A", false, _ => "ok", Completed);
            assert(operation.Error == "work failed", "Failure cleared without a rising edge.");
            operation.Update("A", true, _ => "ok", _ => throw new Exception("format failed"));
            Until(() => !operation.IsPending);
            assert(operation.Status == "Failed" && operation.Error == "format failed" && operation.Value == null,
                "Formatter failure was not observed.");
            operation.Update("B", true, _ => "ok", Completed);
            assert(operation.Error == null && operation.Status == "Run required", "Changed key retained failure.");
            operation.Update("B", false, _ => "ok", Completed);
            operation.Update("B", true, _ => "ok", Completed);
            Until(() => !operation.IsPending);
            assert(operation.Value == "ok" && operation.Error == null, "Recovery failed.");
        });

        test("Async executor synchronous failures are observed", () =>
        {
            using var operation = new AsyncTriggeredOperation<string>((_, _) => throw new Exception("executor failed"));
            operation.Update("A", true, _ => "unused", Completed);
            Until(() => !operation.IsPending);
            assert(operation.Status == "Failed" && operation.Error == "executor failed", "Executor failure lost.");
        });

        test("Async invalidation cancels queued work and preserves edge state", () =>
        {
            using var queued = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            var drained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            int calls = 0;
            using var operation = new AsyncTriggeredOperation<string>((work, token) => Task.Run(() =>
            {
                queued.Set();
                Wait(release);
                try { return work(token); }
                finally { drained.TrySetResult(true); }
            }));
            try
            {
                operation.Update("A", true, _ => { Interlocked.Increment(ref calls); return "ok"; }, Completed);
                Wait(queued);
                long revision = operation.Revision;
                operation.Invalidate();
                assert(operation.Revision > revision && operation.Value == null && !operation.IsPending,
                    "Invalidate did not discard state.");
                operation.Update("A", true, _ => "unexpected", Completed);
                assert(!operation.IsPending, "Invalidate reset the edge.");
                release.Set();
                assert(drained.Task.Wait(Timeout) && calls == 0, "Cancelled queued work executed.");
                operation.Update("A", false, _ => "ok", Completed);
                operation.Update("A", true, _ => "ok", Completed);
                Until(() => !operation.IsPending);
                assert(operation.Value == "ok", "Rearm after invalidate failed.");
            }
            finally { release.Set(); }
        });

        test("Async dispose cancels without waiting for token callbacks", () =>
        {
            using var entered = new ManualResetEventSlim();
            using var callbackEntered = new ManualResetEventSlim();
            using var releaseCallback = new ManualResetEventSlim();
            using var releaseWork = new ManualResetEventSlim();
            using var finished = new ManualResetEventSlim();
            using var operation = new AsyncTriggeredOperation<string>(Execute);
            try
            {
                operation.Update("A", true, token =>
                {
                    using var registration = token.Register(() => { callbackEntered.Set(); Wait(releaseCallback); });
                    entered.Set();
                    Wait(releaseWork);
                    finished.Set();
                    return "late";
                }, Completed);
                Wait(entered);
                var dispose = Task.Run(operation.Dispose);
                assert(dispose.Wait(Timeout), "Dispose blocked on cancellation callbacks.");
                Wait(callbackEntered);
                long revision = operation.Revision;
                operation.Dispose();
                releaseCallback.Set();
                releaseWork.Set();
                Wait(finished);
                assert(operation.Value == null && !operation.IsPending && operation.Status == "Disposed",
                    "Disposed operation published output.");
                assert(operation.Revision == revision, "Repeated dispose changed revision.");
                bool rejected = false;
                try { operation.Update("A", false, _ => "unused", Completed); }
                catch (ObjectDisposedException) { rejected = true; }
                assert(rejected, "Disposed operation accepted an update.");
            }
            finally { releaseCallback.Set(); releaseWork.Set(); }
        });
    }
}
