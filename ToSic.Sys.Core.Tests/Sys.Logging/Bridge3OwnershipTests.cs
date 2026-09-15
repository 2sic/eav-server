using Microsoft.Extensions.Logging;
using ToSic.Sys.Services;

namespace ToSic.Sys.Logging;

[Collection(nameof(LogEventBridgeTests))]
public class Bridge3OwnershipTests
{
    [Fact]
    public void Dispose_CompletesExplicitResult_ExactlyOnce()
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("DisposeOnce");
        long sequence;

        using (var call = root.Fn("once")!)
        {
            sequence = call.Entry!.Sequence;
            call.Done("done");
        }

        var entry = Single(ctx.Store.Snapshot(root)!.Entries, item => item.Sequence == sequence);
        Equal("done", entry.Result);
        True(entry.WrapOpenWasClosed);
        Equal(1, ctx.Store.Snapshot(root)!.Entries.Count(item => item.Sequence == sequence));
    }

    [Fact]
    public void Dispose_ClosesWithoutInventingAResult()
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("DisposeUnspecified");
        long sequence;

        using (var call = root.Fn("unspecified")!)
            sequence = call.Entry!.Sequence;

        var entry = Single(ctx.Store.Snapshot(root)!.Entries, item => item.Sequence == sequence);
        Null(entry.Result);
        True(entry.WrapOpenWasClosed);
    }

    [Theory]
    [InlineData("Do")]
    [InlineData("Quick")]
    [InlineData("Getter")]
    public void ThrowingWrapper_RestoresTheActiveOperation(string wrapper)
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("Wrappers");
        Action action = wrapper switch
        {
            "Do" => () => root.Do(() => throw new InvalidOperationException("Do")),
            "Quick" => () => root.Quick<int>(() => throw new InvalidOperationException("Quick")),
            "Getter" => () => root.Getter<int>(() => throw new InvalidOperationException("Getter")),
            _ => throw new ArgumentOutOfRangeException(nameof(wrapper)),
        };

        Throws<InvalidOperationException>(action);

        using (var after = root.Fn("after")!)
            after.Done("after");

        Null(Single(ctx.Store.Snapshot(root)!.Entries, item => item.Result == "after").ParentOperationId);
    }

    [Fact]
    public void DisconnectedFn_NestsWithParentAndNativeMessage()
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("AutomaticNesting");
        var disconnected = new Log("Tst.Disconnected");
        long parentSequence;
        long childSequence;

        using (ctx.Logger.BeginExecution(root, ctx.Source, "request"))
        using (var parent = root.Fn("parent")!)
        {
            parentSequence = parent.Entry!.Sequence;
            using (var child = disconnected.Fn("child")!)
            {
                childSequence = child.Entry!.Sequence;
                ctx.Logger.LogInformation("native child");
                child.Done("child");
            }
            parent.Done("parent");
        }

        var entries = ctx.Store.Snapshot(root)!.Entries;
        var parentEntry = Single(entries, item => item.Sequence == parentSequence);
        var childEntry = Single(entries, item => item.Sequence == childSequence);
        Equal(0, parentEntry.Depth);
        Equal(1, childEntry.Depth);
        Equal(parentSequence, childEntry.ParentOperationId);
        Equal(childSequence, Single(entries, item => item.Message == "native child").OperationId);
        Null(disconnected.Parent);
    }

    [Fact]
    public void DirectAndLinkLogConnections_DoNotChangeILoggerMembership()
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("Connections");
        var direct = new Log("Tst.Direct", root);
        var owner = new LogOwner("Tst.Linked");
        owner.LinkLog(root);
        var disconnected = new Log("Tst.None");

        Null(direct.Parent);
        Null(owner.TypedLog.Parent);

        using (ctx.Logger.BeginExecution(root, ctx.Source, "request"))
        {
            direct.A("direct");
            owner.Log!.A("linked");
            disconnected.A("disconnected");
        }

        var entries = ctx.Store.Snapshot(root)!.Entries;
        Contains(entries, item => item.Message == "direct");
        Contains(entries, item => item.Message == "linked");
        Contains(entries, item => item.Message == "disconnected");
        Null(disconnected.Parent);
    }

    [Fact]
    public async Task ReusedLogger_RemainsInItsSequentialAndParallelExecutions()
    {
        using var ctx = new LogExecutionTestContext();
        var firstEntry = ctx.AdmitEntry("First");
        var secondEntry = ctx.AdmitEntry("Second");
        var first = (Log)firstEntry.Log!;
        var second = (Log)secondEntry.Log!;
        var reused = new Log("Tst.Reused");

        using (ctx.Logger.BeginExecution(first, ctx.Source, "first"))
            await Call(reused, "first");

        using (ctx.Logger.BeginExecution(second, ctx.Source, "second"))
        {
            await Call(reused, "second");
            await Task.WhenAll(Call(reused, "left"), Call(reused, "right"));
        }

        var snapshots = ctx.Store.Snapshot("test");
        var firstEntries = Single(snapshots, item => item.LogId == firstEntry.ExecutionId).Entries;
        var secondEntries = Single(snapshots, item => item.LogId == secondEntry.ExecutionId).Entries;
        Contains(firstEntries, item => item.Result == "first");
        DoesNotContain(firstEntries, item => item.Result is "second" or "left" or "right");
        foreach (var result in new[] { "second", "left", "right" })
            Contains(secondEntries, item => item.Result == result);
        DoesNotContain(secondEntries, item => item.Result == "first");
        Null(reused.Parent);

        static async Task Call(Log log, string result)
        {
            using var call = log.Fn(result)!;
            await Task.Yield();
            call.Done(result);
        }
    }

    [Fact]
    public async Task ReusedLogger_ConcurrentExecutionsDoNotCrossContaminate()
    {
        using var ctx = new LogExecutionTestContext();
        var first = ctx.AdmitEntry("ConcurrentFirst");
        var second = ctx.AdmitEntry("ConcurrentSecond");
        var reused = new Log("Tst.ConcurrentReused");

        await Task.WhenAll(Run(first, "first"), Run(second, "second"));

        var snapshots = ctx.Store.Snapshot("test");
        var firstEntries = Single(snapshots, item => item.LogId == first.ExecutionId).Entries;
        var secondEntries = Single(snapshots, item => item.LogId == second.ExecutionId).Entries;
        Contains(firstEntries, item => item.Result == "first");
        DoesNotContain(firstEntries, item => item.Result == "second");
        Contains(secondEntries, item => item.Result == "second");
        DoesNotContain(secondEntries, item => item.Result == "first");

        async Task Run(LogStoreEntry execution, string result)
        {
            using var scope = ctx.Logger.BeginExecution(execution, ctx.Source, result);
            using var call = reused.Fn(result)!;
            await Task.Yield();
            call.Done(result);
        }
    }

    [Fact]
    public void OperationsCompletedOutOfOrder_DoNotLeaveStaleAmbientParent()
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("OutOfOrder");
        var reused = new Log("Tst.OutOfOrderReused");

        using (ctx.Logger.BeginExecution(root, ctx.Source, "request"))
        {
            var outer = root.Fn("outer")!;
            var inner = reused.Fn("inner")!;
            outer.Dispose();
            reused.A("still inner");
            inner.Dispose();
            reused.A("after both");
        }

        var entries = ctx.Store.Snapshot(root)!.Entries;
        var innerEntry = Single(entries, item => item.WrapOpen && item.Message!.Contains("inner"));
        Equal(innerEntry.OperationId, Single(entries, item => item.Message == "still inner").OperationId);
        Null(Single(entries, item => item.Message == "after both").OperationId);
    }

    [Fact]
    public void OperationCompletedInLaterExecution_KeepsItsOriginalOwnershipAndScope()
    {
        using var ctx = new LogExecutionTestContext();
        var first = ctx.AdmitEntry("CompletionFirst");
        var second = ctx.AdmitEntry("CompletionSecond");
        var reused = new Log("Tst.DelayedCompletion");
        ILogCall call;

        using (ctx.Logger.BeginExecution(first, ctx.Source, "first", moduleId: 331))
            call = reused.Fn("started in first")!;

        using (ctx.Logger.BeginExecution(second, ctx.Source, "second", moduleId: 333))
        using (var secondCall = ((Log)second.Log!).Fn("second operation")!)
        {
            call.Done("completed in first");
            secondCall.Done("second");
        }

        var snapshots = ctx.Store.Snapshot("test");
        var firstEntry = Single(Single(snapshots, item => item.LogId == first.ExecutionId).Entries,
            item => item.Result == "completed in first");
        Equal("331", firstEntry.Properties["2sxc.ModuleId"]);
        Null(firstEntry.ParentOperationId);
        DoesNotContain(Single(snapshots, item => item.LogId == second.ExecutionId).Entries,
            item => item.Result == "completed in first");
    }

    [Fact]
    public async Task ExplicitCallOwnership_SurvivesSuppressedExecutionFlow()
    {
        using var ctx = new LogExecutionTestContext();
        var execution = ctx.AdmitEntry("ExplicitOwnership");
        ILogCall parent;
        using (ctx.Logger.BeginExecution(execution, ctx.Source, "request"))
            parent = execution.Log!.Fn("parent")!;

        Task childWork;
        using (ExecutionContext.SuppressFlow())
            childWork = Task.Run(() =>
            {
                var linked = new Log("Tst.ExplicitLinked", parent);
                using var child = linked.Fn("child")!;
                child.Done("child");
            });
        await childWork;
        parent.Done("parent");

        var entries = Single(ctx.Store.Snapshot("test"), item => item.LogId == execution.ExecutionId).Entries;
        var childEntry = Single(entries, item => item.Result == "child");
        Equal(parent.Entry!.Sequence, childEntry.ParentOperationId);
        Equal(1, childEntry.Depth);
    }

    [Fact]
    public async Task BeginInvocation_UsesParentsCapturedExecutionWithoutAmbientFlow()
    {
        using var ctx = new LogExecutionTestContext();
        var execution = ctx.AdmitEntry("InvocationOwnership");
        ILogCall parent;
        using (ctx.Logger.BeginExecution(execution, ctx.Source, "request"))
            parent = execution.Log!.Fn("parent")!;

        Task childWork;
        using (ExecutionContext.SuppressFlow())
            childWork = Task.Run(() =>
            {
                using var invocation = ctx.Logger.BeginInvocation(parent);
                using var child = new Log("Tst.InvocationChild").Fn("child")!;
                child.Done("child");
            });
        await childWork;
        parent.Done("parent");

        var entries = Single(ctx.Store.Snapshot("test"), item => item.LogId == execution.ExecutionId).Entries;
        var childEntry = Single(entries, item => item.Result == "child");
        Equal(parent.Entry!.Sequence, childEntry.ParentOperationId);
        Equal(1, childEntry.Depth);
    }

    [Fact]
    public void PendingReplay_KeepsTheEventTimeExecutionInsteadOfTheAdmissionExecution()
    {
        using var ctx = new LogExecutionTestContext();
        var first = ctx.Admit("PendingFirst");
        var second = ctx.Admit("PendingSecond");
        var pending = new Log("Tst.Pending");

        using (ctx.Logger.BeginExecution(first, ctx.Source, "first", moduleId: 331))
            pending.A("pending event");

        using (ctx.Logger.BeginExecution(second, ctx.Source, "second", moduleId: 333))
            ctx.Store.Add("test", pending);

        var entry = Single(ctx.Store.Snapshot(pending)!.Entries, item => item.Message == "pending event");
        Equal("331", entry.Properties["2sxc.ModuleId"]);
        Null(ctx.Store.Snapshot(second));
    }

    [Fact]
    public void AdmissionInsideUnadmittedFn_OwnsFollowingEntriesWithoutPending()
    {
        LogEventBridge.SetSink(null);
        var unadmitted = new Log("Tst.Unadmitted");
        using var outer = unadmitted.Fn("outer")!;
        using var ctx = new LogExecutionTestContext();

        var admittedLog = new Log("Tst.InsideUnadmitted", outer);
        var beforeAdmission = admittedLog.Fn("before admission")!;
        var beforeAdmissionSequence = beforeAdmission.Entry!.Sequence;
        admittedLog.A("pre-admission child");
        var admitted = ctx.Store.Add("test", admittedLog)!;
        using (var inner = admitted.Log!.Fn("inner")!)
            inner.Done("done");
        new Log("Tst.Child").A("child after admission");
        beforeAdmission.Done("adopted");

        var snapshot = Single(ctx.Store.Snapshot("test"));
        Equal(admitted.ExecutionId, snapshot.LogId);
        var adopted = Single(snapshot.Entries, entry => entry.Sequence == beforeAdmissionSequence);
        Equal("adopted", adopted.Result);
        Equal(admitted.ExecutionId, adopted.RootLogId);
        Null(adopted.ParentOperationId);
        Equal(0, adopted.Depth);
        Equal(1, Single(snapshot.Entries, entry => entry.Message == "pre-admission child").Depth);
        Equal("done", Single(snapshot.Entries, entry => entry.Result == "done").Result);
        var child = Single(snapshot.Entries, entry => entry.Message == "child after admission");
        Equal(admitted.ExecutionId, child.RootLogId);
        Equal(beforeAdmissionSequence, child.OperationId);
        Equal(1, child.Depth);
        Contains($"0/{InsightsLogStore.MaxPendingEntries} pending", ctx.Store.Status);
    }

    [Fact]
    public async Task ConcurrentLateAdmissions_KeepTheirOwnActiveOperation()
    {
        LogEventBridge.SetSink(null);
        var unadmitted = new Log("Tst.ConcurrentOuter");
        using var outer = unadmitted.Fn("outer")!;
        using var ctx = new LogExecutionTestContext();
        var shared = new Log("Tst.ConcurrentLate");
        var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = 0;

        var admissions = await Task.WhenAll(Run("first"), Run("second"));

        var snapshots = ctx.Store.Snapshot("test");
        var results = new[] { "first", "second" };
        for (var i = 0; i < admissions.Length; i++)
        {
            var entries = Single(snapshots, snapshot => snapshot.LogId == admissions[i].ExecutionId).Entries;
            Contains(entries, entry => entry.Result == results[i]);
            DoesNotContain(entries, entry => entry.Result != null && entry.Result != results[i]);
        }
        Contains($"0/{InsightsLogStore.MaxPendingEntries} pending", ctx.Store.Status);

        async Task<LogStoreEntry> Run(string result)
        {
            using var call = shared.Fn(result)!;
            if (Interlocked.Increment(ref started) == 2)
                ready.SetResult(true);
            await ready.Task;
            var admission = ctx.Store.Add("test", shared)!;
            await Task.Yield();
            call.Done(result);
            return admission;
        }
    }

    [Fact]
    public void PreSinkHistory_IsReplayedOnlyIntoTheFirstAdmission()
    {
        LogEventBridge.SetSink(null);
        var pending = new Log("Tst.PreSink");
        pending.A("before sink");
        using var ctx = new LogExecutionTestContext();

        var first = ctx.Store.Add("test", pending)!;
        var second = ctx.Store.Add("test", pending)!;

        var snapshots = ctx.Store.Snapshot("test");
        Equal(1, snapshots.Sum(snapshot => snapshot.Entries.Count(entry => entry.Message == "before sink")));
        Contains(Single(snapshots, snapshot => snapshot.LogId == first.ExecutionId).Entries,
            entry => entry.Message == "before sink");
        DoesNotContain(Single(snapshots, snapshot => snapshot.LogId == second.ExecutionId).Entries,
            entry => entry.Message == "before sink");
    }

    [Fact]
    public void LongLivedUnadmittedLog_IsBoundedAndReplaysTheRetainedTail()
    {
        using var ctx = new LogExecutionTestContext();
        var pending = new Log("Tst.LongLived");

        for (var i = 0; i <= InsightsLogStore.MaxPendingEntries; i++)
            pending.A($"pending {i}");

        Equal(InsightsLogStore.MaxEntriesPerLog, pending.Entries.Count);
        Contains($"{InsightsLogStore.MaxPendingEntries}/{InsightsLogStore.MaxPendingEntries} pending", ctx.Store.Status);

        ctx.Store.Add("test", pending);

        var entries = ctx.Store.Snapshot(pending)!.Entries;
        Equal(InsightsLogStore.MaxPendingEntries, entries.Length);
        DoesNotContain(entries, item => item.Message == "pending 0");
        Contains(entries, item => item.Message == $"pending {InsightsLogStore.MaxPendingEntries}");
        Contains("1 dropped events", ctx.Store.Status);
    }

    private sealed class LogOwner(string name) : IHasLog
    {
        internal Log TypedLog { get; } = new(name);
        public ILog? Log => TypedLog;
    }

}
