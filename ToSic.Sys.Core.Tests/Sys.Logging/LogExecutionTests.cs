using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

[Collection(nameof(LogEventBridgeTests))]
public class LogExecutionTests
{
    #region Execution context and replay
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BeginExecution_RestoresOuterContext_WithOrWithoutActivityListener(bool listen)
    {
        using var ctx = new LogExecutionTestContext();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => listen && source == ctx.Source,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        var originalActivity = Activity.Current;
        var outer = ctx.Admit("Outer");
        var inner = ctx.Admit("Inner");
        var reused = new Log("Tst.Reused");

        using (ctx.Logger.BeginExecution(outer, ctx.Source, "outer", siteId: 1, moduleId: 331))
        {
            var outerActivity = Activity.Current;
            try
            {
                using var scope = ctx.Logger.BeginExecution(inner, ctx.Source, "inner", moduleId: 333);
                await Task.Yield();
                reused.A("inner");
                ctx.Logger.LogInformation("native inner");
                if (listen)
                    Equal(outerActivity!.SpanId, Activity.Current!.ParentSpanId);
                throw new OperationCanceledException();
            }
            catch (OperationCanceledException)
            {
                reused.A("outer restored");
            }
            Same(outerActivity, Activity.Current);
        }
        reused.A("outside");

        Same(originalActivity, Activity.Current);
        var innerEntry = Single(ctx.Store.Snapshot(inner)!.Entries, e => e.Message == "inner");
        Equal("333", innerEntry.Properties["2sxc.ModuleId"]);
        Equal("1", innerEntry.Properties["2sxc.SiteId"]);
        Contains(ctx.Store.Snapshot(outer)!.Entries, e => e.Message == "outer restored");
        DoesNotContain(ctx.Store.Snapshot(outer)!.Entries, e => e.Message is "inner" or "outside");
        Null(reused.Parent);
    }

    [Fact]
    public void Replay_DoesNotImportHistoryIntoTheCurrentExecution()
    {
        using var ctx = new LogExecutionTestContext();
        var first = ctx.Admit("First");
        var second = ctx.Admit("Second");
        var child = new Log("Tst.History");
        using (ctx.Logger.BeginExecution(first, ctx.Source, "first", moduleId: 331))
            child.A("historical event");

        using (ctx.Logger.BeginExecution(second, ctx.Source, "second", moduleId: 333))
            ctx.Store.Add("test", child);

        DoesNotContain(ctx.Store.Snapshot(second)!.Entries, e => e.Message == "historical event");
        Equal("331", Single(ctx.Store.Snapshot(first)!.Entries).Properties["2sxc.ModuleId"]);
        False(Single(ctx.Store.Snapshot(child)!.Entries).Properties.ContainsKey("2sxc.ModuleId"));
    }

    [Fact]
    public void Replay_DoesNotCopyOneBundlesCapturedContext_IntoAnotherBundle()
    {
        using var ctx = new LogExecutionTestContext();
        var first = ctx.Admit("First");
        var second = ctx.Admit("Second");
        using (ctx.Logger.BeginExecution(first, ctx.Source, "first", moduleId: 331))
            first.A("original event");
        var original = Single(ctx.Store.Snapshot(first)!.Entries);
        // A late replay carries source data but cannot recover its publication-time scope.
        var replay = original with { Ancestors = [second.LogId], Properties = original.Properties.Clear(), Replay = true };

        ctx.Logger.Log(LogLevel.Trace, default, replay, null, (entry, _) => entry.ToString());

        Equal("331", Single(ctx.Store.Snapshot(first)!.Entries).Properties["2sxc.ModuleId"]);
        False(Single(ctx.Store.Snapshot(second)!.Entries).Properties.ContainsKey("2sxc.ModuleId"));
    }

    [Fact]
    public void BeginExecution_UsesAdmittedRoot_ForLinkedBoundaryLogger()
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("Root");
        var boundary = new Log("Tst.Boundary", parent: root);

        using (ctx.Logger.BeginExecution(boundary, ctx.Source, "boundary"))
            boundary.A("inside linked boundary");

        Contains(ctx.Store.Snapshot(root)!.Entries, e => e.Message == "inside linked boundary");
    }

    [Fact]
    public async Task BeginInvocation_ParentsIndependentCalls_WithoutSequentialOrParallelLeakage()
    {
        using var ctx = new LogExecutionTestContext();
        var firstRoot = ctx.Admit("First");
        var secondRoot = ctx.Admit("Second");
        var reused = new Log("Tst.Reused");

        var firstParent = firstRoot.Fn("first parent");
        using (ctx.Logger.BeginInvocation(firstParent))
            await ServiceCall(reused, "first");
        firstParent.Done();

        var secondParent = secondRoot.Fn("second parent");
        using (ctx.Logger.BeginInvocation(secondParent))
        {
            await ServiceCall(reused, "second");
            await ServiceCall(reused, "repeated");
            await Task.WhenAll(
                ServiceCall(reused, "left"),
                ServiceCall(reused, "right"));
        }
        secondParent.Done();

        var first = ctx.Store.Snapshot(firstRoot)!.Entries;
        var second = ctx.Store.Snapshot(secondRoot)!.Entries;
        Equal(firstParent.Entry!.Sequence, Single(first, e => e.Result == "first").ParentOperationId);
        DoesNotContain(first, e => e.Result is "second" or "repeated" or "left" or "right");
        foreach (var name in new[] { "second", "repeated", "left", "right" })
            Equal(secondParent.Entry!.Sequence, Single(second, e => e.Result == name).ParentOperationId);
        Null(reused.Parent);

        static async Task ServiceCall(Log log, string name)
        {
            var call = log.Fn(name);
            await Task.Yield();
            call.Done(name);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BeginInvocation_PreservesRootAndOperationTree_WithNestedIndependentServices(bool boundary)
    {
        using var ctx = new LogExecutionTestContext();
        var root = ctx.Admit("Root");
        var service = new Log("Tst.Service");
        var leaf = new Log("Tst.Leaf");
        using var execution = boundary ? ctx.Logger.BeginExecution(root, ctx.Source, "request") : null;
        var parent = root.Fn("parent");
        using (ctx.Logger.BeginInvocation(parent))
        {
            var child = service.Fn("child");
            using (ctx.Logger.BeginInvocation(child))
            {
                await Task.Yield();
                var grandchild = leaf.Fn("grandchild");
                grandchild.Done("grandchild result");
                ctx.Logger.LogInformation("native child message");
                new Log("Tst.Message").A("independent child message");
                child.Done("child result");
            }
            ctx.Logger.LogInformation("native parent message");
            parent.Done("parent result");
        }

        var entries = ctx.Store.Snapshot(root)!.Entries;
        var parentEntry = Single(entries, e => e.Result == "parent result");
        var childEntry = Single(entries, e => e.Result == "child result");
        var grandchildEntry = Single(entries, e => e.Result == "grandchild result");
        Null(parentEntry.ParentOperationId);
        Equal(parentEntry.OperationId, childEntry.ParentOperationId);
        Equal(childEntry.OperationId, grandchildEntry.ParentOperationId);
        Equal(service.LogId, childEntry.LogId);
        Equal(leaf.LogId, grandchildEntry.LogId);
        Equal(childEntry.OperationId, Single(entries, e => e.Message == "native child message").OperationId);
        Equal(childEntry.OperationId, Single(entries, e => e.Message == "independent child message").OperationId);
        Equal(parentEntry.OperationId, Single(entries, e => e.Message == "native parent message").OperationId);
        Null(service.Parent);
        Null(leaf.Parent);
    }

    [Fact]
    public async Task BeginInvocation_RestoresParent_AfterNestedExecutionThrows()
    {
        using var ctx = new LogExecutionTestContext();
        var outer = ctx.Admit("Outer");
        var inner = ctx.Admit("Inner");
        using var execution = ctx.Logger.BeginExecution(outer, ctx.Source, "outer");
        var parent = outer.Fn("parent");
        using (ctx.Logger.BeginInvocation(parent))
        {
            try
            {
                using var nested = ctx.Logger.BeginExecution(inner, ctx.Source, "inner");
                await Task.Yield();
                ctx.Logger.LogInformation("inner message");
                throw new OperationCanceledException();
            }
            catch (OperationCanceledException)
            {
                ctx.Logger.LogInformation("restored parent message");
            }
            parent.Done();
        }
        ctx.Logger.LogInformation("outside invocation");

        var innerEntry = Single(ctx.Store.Snapshot(inner)!.Entries);
        Null(innerEntry.OperationId);
        var entries = ctx.Store.Snapshot(outer)!.Entries;
        Equal(parent.Entry!.Sequence, Single(entries, e => e.Message == "restored parent message").OperationId);
        Null(Single(entries, e => e.Message == "outside invocation").OperationId);
        DoesNotContain(entries, e => e.Message == "inner message");
    }

    #endregion
}
