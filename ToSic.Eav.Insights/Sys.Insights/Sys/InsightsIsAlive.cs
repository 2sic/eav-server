namespace ToSic.Eav.Sys.Insights.Sys;

internal class InsightsIsAlive()
    : InsightsProvider(new() { Name = Link, Title = "Is Alive Check / Ping" })
{
    public static string Link = "IsAlive";

    public override string HtmlBody() => true.ToString();
}