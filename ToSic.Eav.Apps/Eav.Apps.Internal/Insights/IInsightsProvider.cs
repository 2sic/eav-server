namespace ToSic.Eav.Apps.Internal.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IInsightsProvider
{
    string HelpCategory { get; }

    string Name { get; }

    string Teaser { get; }

    string Title { get; }

    void SetContext(IInsightsLinker linker, int? appId, IDictionary<string, object> parameters, string key, int? position, string type, bool? toggle, string nameId, string filter);

    string HtmlBody();
}