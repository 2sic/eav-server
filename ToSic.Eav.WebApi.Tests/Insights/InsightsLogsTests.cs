using static Xunit.Assert;
using System.Text.RegularExpressions;
using ToSic.Eav.Sys.Insights.Logs;
using ToSic.Sys.Logging;

namespace ToSic.Eav.Insights;

[CollectionDefinition(nameof(InsightsLogsTests), DisableParallelization = true)]
public sealed class InsightsLogsTestCollection;

[Collection(nameof(InsightsLogsTests))]
public class InsightsLogsTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(1)]
    public void DumpTree_UsesLatestCompletion_ForTotalTimeAndSpecs(int measuredSeconds)
    {
        var start = new DateTime(2026, 9, 11, 0, 0, 0, DateTimeKind.Utc);
        var snapshot = new LogSnapshot
        {
            LogId = "timing",
            Created = start,
            Entries =
            [
                new()
                {
                    Sequence = 1, OperationId = 1, Source = "Root", Message = "outer",
                    Created = start, Completed = start.AddSeconds(10),
                    WrapOpen = true, WrapOpenWasClosed = true, IsTimed = true,
                    Elapsed = TimeSpan.FromSeconds(measuredSeconds),
                },
                new() { Sequence = 2, OperationId = 1, Source = "Root", Message = "last message", Created = start.AddSeconds(2) },
            ],
        };
        using var ctx = new InsightsLogsTestContext();
        ctx.Write(new() { Kind = "Admission", LogId = snapshot.LogId, Segment = "test", Created = start });
        foreach (var entry in snapshot.Entries)
            ctx.Write(entry with { LogId = snapshot.LogId });
        ctx.SetContext(logId: snapshot.LogId);

        var html = ctx.View.HtmlBody();

        var percentage = Regex.Match(html, @"class=['""]time-of-total['""]>(\d+)%").Groups[1].Value;
        Equal((measuredSeconds * 10).ToString(), percentage);
        Contains(start.AddSeconds(10).ToLocalTime().ToString("o"), html);
        Contains("00:00:10", html);
    }

    #region Log selection
    [Fact]
    public void FilteredLink_SelectsStableIdentity_AndExpiresAfterEviction()
    {
        using var ctx = new InsightsLogsTestContext();
        ctx.Add("unselected", "1");
        var selected = ctx.Add("selected event", "2");
        var selectedId = ctx.StoredId(selected);
        ctx.SetContext(filter: "ModuleId=2");

        var list = ctx.View.HtmlBody();

        Contains($"nameId={selectedId}", list);
        DoesNotContain("position=1", list);
        ctx.SetContext(logId: selectedId);
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
        store.Configure(null);

        var html = new InsightsLogsHelper(store).DumpTree("Global Types", new Log("Tst.Types"));

        Contains("not captured in this store — only admitted logs are retained", html);
    }

    [Fact]
    public void DumpTree_PreservesSubtreeAndLocalTime_AfterIndexedBundleIsFlushed()
    {
        using var ctx = new InsightsLogsTestContext();
        var root = ctx.Add("root entry", "1");
        var leaf = new Log("Tst.Leaf");
        using (var parent = root.Fn("parent"))
        using (var child = leaf.Fn("leaf entry", timer: true))
        {
            child.Done("leaf result");
            parent.Done();
        }
        ctx.Store.Add("middle", leaf);
        ctx.Store.FlushSegment("middle");

        var helper = new InsightsLogsHelper(ctx.Store);
        var html = helper.DumpTree("Subtree", leaf);
        var snapshot = ctx.Store.Snapshot(leaf)!;

        Contains("leaf entry", html);
        Contains("leaf result", html);
        DoesNotContain("root entry", html);
        Contains(snapshot.Created.ToLocalTime().ToString("o"), html);
        Contains(root.Created.ToLocalTime().ToString("O").Substring(5), helper.LogHistoryList("test", ""));
        ctx.Store.FlushSegment("test");
        Contains("not captured in this store", helper.DumpTree("Subtree", leaf));
    }
    #endregion
}
