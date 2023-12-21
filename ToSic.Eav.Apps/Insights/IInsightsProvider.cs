using System.Collections.Generic;

namespace ToSic.Eav.Apps.Insights;

public interface IInsightsProvider
{
    string Category { get; }

    string Name { get; }

    string Teaser { get; }

    string Title { get; }

    void SetContext(IInsightsLinker linker, int? appId = default, IDictionary<string, object> parameters = default);

    string HtmlBody();
}