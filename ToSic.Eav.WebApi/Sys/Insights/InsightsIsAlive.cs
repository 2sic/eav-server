using ToSic.Eav.Apps.Internal.Insights;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsIsAlive(): InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay)
{
    public static string Link = "IsAlive";

    public override string HtmlBody() => true.ToString();
}