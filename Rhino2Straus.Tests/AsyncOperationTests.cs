#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;

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
        test("Load duplicates and copy constructors isolate coordinate systems", () =>
        {
            var axes = new CoordinateSystemModel("Axes", new Point3d(1, 2, 3), new Point3d(4, 5, 6), new Point3d(7, 8, 9), 12);
            var frame = new FrameLoadStrausModel { CoordinateSystem = axes, PA = 17, P2 = 23 };
            var area = new AreaPressureLoadModel(new LoadCaseModel("Q"), 31) { CoordinateSystem = axes };
            var frameDuplicate = (FrameLoadStrausModel)frame.Duplicate();
            var frameCopy = new FrameLoadStrausModel(frame);
            var areaDuplicate = (AreaPressureLoadModel)area.Duplicate();
            var areaCopy = new AreaPressureLoadModel(area);
            var copies = new[] { frameDuplicate.CoordinateSystem, frameCopy.CoordinateSystem,
                areaDuplicate.CoordinateSystem, areaCopy.CoordinateSystem };
            foreach (var copy in copies)
                assert(!ReferenceEquals(copy, axes) && copy == axes, "Coordinate system copy lost values or retained its source.");
            axes.Id = 99; axes.Name = "Changed";
            axes.Origin = Point3d.Origin; axes.Point1 = Point3d.Origin; axes.Point2 = Point3d.Origin;
            foreach (var copy in copies)
                assert(copy.Id == 12 && copy.Name == "Axes" && copy.Origin == new Point3d(1, 2, 3)
                    && copy.Point1 == new Point3d(4, 5, 6) && copy.Point2 == new Point3d(7, 8, 9),
                    "Source edits changed a copied coordinate system.");
            frameCopy.CoordinateSystem.Name = "Copy only";
            assert(frameDuplicate.CoordinateSystem.Name == "Axes" && axes.Name == "Changed",
                "Independent load copies share coordinate systems.");
            assert(frameDuplicate.PA == 17 && frameCopy.P2 == 23 && areaDuplicate.Value == 31 && areaCopy.Value == 31,
                "Copying coordinates changed load values or runtime types.");
        });

        test("Coordinate copies retain Local and Global semantics and optional nulls", () =>
        {
            foreach (var axes in new[] { CoordinateSystemModel.Local, CoordinateSystemModel.Global })
            {
                var frame = new FrameLoadStrausModel { CoordinateSystem = axes };
                var area = new AreaPressureLoadModel { CoordinateSystem = axes };
                foreach (var copy in new[] { frame.Duplicate().CoordinateSystem, new FrameLoadStrausModel(frame).CoordinateSystem,
                    area.Duplicate().CoordinateSystem, new AreaPressureLoadModel(area).CoordinateSystem })
                    assert(copy == axes && !ReferenceEquals(copy, axes), "Local/Global equality or isolation changed.");
            }
            var emptyFrame = new FrameLoadStrausModel();
            var emptyArea = new AreaPressureLoadModel();
            assert(emptyFrame.Duplicate().CoordinateSystem == null && new FrameLoadStrausModel(emptyFrame).CoordinateSystem == null
                && emptyArea.Duplicate().CoordinateSystem == null && new AreaPressureLoadModel(emptyArea).CoordinateSystem == null,
                "Absent coordinate systems must remain absent.");
            assert(new NodeSupportModel(new NodeSupportModel()).BoundaryGroup == null
                && new LinkPropertyModel(new LinkPropertyModel()).BoundaryGroup == null
                && new NodalLoadModel(new NodalLoadModel()).LoadGroup == null,
                "Absent group metadata must remain absent.");
        });

        test("Support and link copies isolate boundary group identity", () =>
        {
            var group = new BoundaryGroupModel("Boundary") { Id = 42 };
            var support = new NodeSupportModel(true, false, true, false, true, false) { BoundaryGroup = group };
            var link = new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.GEN, 11, 12, 13, 14, 15, 16) { BoundaryGroup = group };
            var supportCopy = new NodeSupportModel(support);
            var linkCopy = new LinkPropertyModel(link);
            assert(!ReferenceEquals(group, supportCopy.BoundaryGroup) && !ReferenceEquals(group, linkCopy.BoundaryGroup),
                "Boundary group remained shared with the input.");
            group.Name = "Edited"; group.Id = 77;
            assert(supportCopy.BoundaryGroup.Name == "Boundary" && supportCopy.BoundaryGroup.Id == 42
                && linkCopy.BoundaryGroup.Name == "Boundary" && linkCopy.BoundaryGroup.Id == 42,
                "Source edits changed copied boundary metadata.");
            supportCopy.BoundaryGroup.Name = "Support only";
            assert(linkCopy.BoundaryGroup.Name == "Boundary" && group.Name == "Edited", "Copies share boundary metadata.");
            assert(supportCopy.Dx && !supportCopy.Dy && supportCopy.Dz && !supportCopy.Mx && supportCopy.My && !supportCopy.Mz
                && linkCopy.Type == link.Type && linkCopy.Kx == 11 && linkCopy.Ky == 12 && linkCopy.Kz == 13
                && linkCopy.Rx == 14 && linkCopy.Ry == 15 && linkCopy.Rz == 16, "Support or link values changed during copy.");
        });

        test("Nodal load copies isolate group name ID and suffix", () =>
        {
            var group = new LoadGroupModel("Loads", "original") { Id = 24 };
            var source = new NodalLoadModel(new LoadCaseModel("Q"), 1, 2, 3, 4, 5, 6) { LoadGroup = group };
            var first = new NodalLoadModel(source);
            var second = new NodalLoadModel(source);
            assert(!ReferenceEquals(first.LoadGroup, group), "LoadGroup remained shared with the source.");
            group.Name = "Changed"; group.Id = 25; group.Suffix = "edited";
            assert(first.LoadGroup.Name == "Loads" && first.LoadGroup.Id == 24 && first.LoadGroup.Suffix == "original",
                "Source edits changed copied load group metadata.");
            first.LoadGroup.Suffix = "copy";
            assert(second.LoadGroup.Suffix == "original" && group.Suffix == "edited", "Copies share LoadGroup metadata.");
            assert(first.FX == 1 && first.FY == 2 && first.FZ == 3 && first.MX == 4 && first.MY == 5 && first.MZ == 6,
                "Nodal load values changed during group copy.");
        });

        test("Queued Model snapshot retains identity after source metadata edits", () =>
        {
            var loadCase = new LoadCaseModel("Q");
            var n1 = new NodeElementModel(0, 0, 0) { Id = 1 };
            var n2 = new NodeElementModel(1, 0, 0) { Id = 2 };
            var n3 = new NodeElementModel(0, 1, 0) { Id = 3 };
            n1.Support.BoundaryGroup = new BoundaryGroupModel("Support") { Id = 10 };
            n2.NodalLoadList.Add(new NodalLoadModel(loadCase, 1, 2, 3, 4, 5, 6)
                { LoadGroup = new LoadGroupModel("Loads", "Q") { Id = 11 } });
            var section = new FrameSectionModel { Name = "Numerical", SectionGeometryType = FrameSectionModel.SectionGeometryTypes.Generic,
                SectionArea = 1, I11 = 1, I22 = 1, J = 1, Breps = new() };
            var beam = new FrameElementModel(n1, n2, new FramePropertyModel("Beam", MaterialModel.S355, section), 0, Vector3d.Zero) { Id = 1 };
            beam.FrameLoadList.Add(new FrameLoadStrausModel { LoadCase = loadCase, CoordinateSystem = new CoordinateSystemModel(CoordinateSystemModel.Local), PA = 2 });
            var plate = new AreaElementModel(new() { n1, n2, n3 },
                new AreaPropertyModel("Plate", MaterialModel.S355, new AreaThicknessModel("t", .2))) { Id = 1 };
            plate.AreaLoadList.Add(new AreaPressureLoadModel(loadCase, 3) { CoordinateSystem = new CoordinateSystemModel(CoordinateSystemModel.Global) });
            var link = new LinkElementModel(n1, n3, new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.GEN, 100)
                { BoundaryGroup = new BoundaryGroupModel("Link") { Id = 12 } }) { Id = 1 };
            var input = new ModelModel();
            input.BuildModel(inputFrames: new() { beam }, inputAreas: new() { plate }, inputLinks: new() { link });
            string fingerprint = ModelFingerprint.Compute(input);
            using var queued = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var drained = new ManualResetEventSlim();
            using var operation = new AsyncTriggeredOperation<ModelModel>((work, token) => Task.Run(() =>
            {
                try { queued.Set(); Wait(release); return work(token); }
                finally { drained.Set(); }
            }));
            try
            {
                operation.UpdatePrepared(fingerprint, true, () =>
                {
                    var snapshot = new ModelModel(input);
                    assert(ModelFingerprint.Compute(snapshot) == fingerprint, "Snapshot changed the initial Model identity.");
                    return _ => snapshot;
                }, _ => "Completed");
                Wait(queued);
                input.FrameElements.Values.Single().FrameLoadList[0].CoordinateSystem.Origin = new Point3d(4, 5, 6);
                input.AreaElements.Values.Single().AreaLoadList[0].CoordinateSystem.Name = "Changed axes";
                input.NodeElements[1].Support.BoundaryGroup.Name = "Changed support";
                input.NodeElements[2].NodalLoadList[0].LoadGroup.Suffix = "Changed loads";
                input.LinkElements.Values.Single().LinkProperty.BoundaryGroup.Id = 99;
                assert(ModelFingerprint.Compute(input) != fingerprint, "Fixture edits did not affect source identity.");
                release.Set();
                Until(() => !operation.IsPending);
                var result = operation.Value;
                assert(result != null && operation.Status == "Completed", "Queued snapshot did not complete: " + operation.Error);
                assert(result != null && ModelFingerprint.Compute(result) == fingerprint,
                    "Queued snapshot inherited source edits through shared metadata.");
                if (result != null)
                {
                    result.NodeElements[2].NodalLoadList[0].LoadGroup.Name = "Snapshot only";
                    assert(input.NodeElements[2].NodalLoadList[0].LoadGroup.Name == "Loads", "Snapshot edits leaked back to the input.");
                }
            }
            finally { release.Set(); Wait(drained); }
        });

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
