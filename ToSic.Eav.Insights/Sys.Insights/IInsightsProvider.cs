namespace ToSic.Eav.Sys.Insights;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IInsightsProvider
{
    string HelpCategory { get; }

    string Name { get; }

    string Teaser { get; }

    string Title { get; }

    void SetContext(IInsightsLinker linker, int? appId, IDictionary<string, object> parameters, string key, int? position, string type, bool? toggle, string nameId, string filter);

    string HtmlBody();
}