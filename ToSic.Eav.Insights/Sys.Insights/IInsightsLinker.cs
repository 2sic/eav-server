namespace ToSic.Eav.Sys.Insights;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IInsightsLinker
{
    string LinkTo(string label, string view, int? appId = null, NoParamOrder npo = default,
        string? key = null, string? type = null, string? nameId = null, string? more = null);

    string LinkTo(string name, NoParamOrder npo = default, string? label = default, string? parameters = default);

    string LinkBack();
}