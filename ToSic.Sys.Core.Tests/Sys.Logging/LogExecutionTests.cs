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

    #endregion
}
