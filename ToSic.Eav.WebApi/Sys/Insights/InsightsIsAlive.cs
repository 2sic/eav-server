using ToSic.Eav.Apps.Internal.Insights;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsIsAlive(): InsightsProvider("IsAlive", helpCategory: HiddenFromAutoDisplay)
{

    public override string HtmlBody() => true.ToString();
}