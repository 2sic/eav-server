using static Xunit.Assert;

namespace ToSic.Eav.Insights;

public class InsightsLogsTests
{
    #region Log selection
    [Fact]
    public void FilteredLink_SelectsStableIdentity_AndExpiresAfterEviction()
    {
        using var ctx = new InsightsLogsTestContext();
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
    #endregion
}
