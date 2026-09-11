using static Xunit.Assert;
using ToSic.Eav.Sys.Insights.Logs;
using ToSic.Sys.Logging;

namespace ToSic.Eav.Insights;

[CollectionDefinition(nameof(InsightsLogsTests), DisableParallelization = true)]
public sealed class InsightsLogsTestCollection;

[Collection(nameof(InsightsLogsTests))]
public class InsightsLogsTests
{
    #region Log selection
    [Theory]
    [InlineData(LogStoreMode.Legacy)]
    [InlineData(LogStoreMode.ILogger)]
    public void FilteredLink_SelectsStableIdentity_AndExpiresAfterEviction(LogStoreMode mode)
    {
        using var ctx = new InsightsLogsTestContext(mode);
        ctx.Add("unselected", "1");
        var selected = ctx.Add("selected event", "2");
        ctx.SetContext(filter: "ModuleId=2");

        var list = ctx.View.HtmlBody();

        Contains($"nameId={selected.LogId}", list);
        DoesNotContain("position=1", list);
        ctx.SetContext(logId: selected.LogId);
        Contains("selected event", ctx.View.HtmlBody());
        ctx.Add("replacement one", "3");
        ctx.Add("replacement two", "4");
        Contains("expired or flushed", ctx.View.HtmlBody());
        DoesNotContain("replacement", ctx.View.HtmlBody());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    public void LegacyPosition_RejectsOutOfRange_InsteadOfOpeningAnotherLog(int position)
    {
        using var ctx = new InsightsLogsTestContext();
        ctx.Add("existing event", "1");
        ctx.SetContext(position: position);

        var detail = ctx.View.HtmlBody();

        Contains("outside log count", detail);
        DoesNotContain("existing event", detail);
    }

    [Fact]
    public void DumpTree_ShowsNotCapturedMessage_ForUnadmittedLog()
    {
        var store = new LogStoreLive();
        store.Configure("ILogger", bridgeEnabled: true);

        var html = new InsightsLogsHelper(store).DumpTree("Global Types", new Log("Tst.Types"));

        Contains("not captured in this store — only admitted logs are retained", html);
    }

    [Fact]
    public void DumpTree_PreservesSubtreeAndLocalTime_AfterIndexedBundleIsFlushed()
    {
        using var ctx = new InsightsLogsTestContext(LogStoreMode.ILogger);
        var root = ctx.Add("root entry", "1");
        var middle = new Log("Tst.Middle", root);
        ctx.Store.Add("middle", middle);
        var leaf = new Log("Tst.Leaf", middle);
        leaf.Fn("leaf entry", timer: true).Done("leaf result");
        new Log("Tst.Sibling", root).A("sibling entry");
        ctx.Store.FlushSegment("middle");

        var helper = new InsightsLogsHelper(ctx.Store);
        var html = helper.DumpTree("Subtree", middle);
        var snapshot = ctx.Store.Snapshot(middle)!;

        Contains("leaf entry", html);
        Contains("leaf result", html);
        DoesNotContain("sibling entry", html);
        DoesNotContain("root entry", html);
        Contains(snapshot.Created.ToLocalTime().ToString("o"), html);
        Contains(root.Created.ToLocalTime().ToString("O").Substring(5), helper.LogHistoryList("test", ""));
        ctx.Store.FlushSegment("test");
        Contains("not captured in this store", helper.DumpTree("Subtree", middle));
    }
    #endregion
}
