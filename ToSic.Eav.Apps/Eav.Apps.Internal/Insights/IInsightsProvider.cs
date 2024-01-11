using System.Collections.Generic;

namespace ToSic.Eav.Apps.Internal.Insights;

public interface IInsightsProvider
{
    string HelpCategory { get; }

    string Name { get; }

    string Teaser { get; }

    string Title { get; }

    void SetContext(IInsightsLinker linker, int? appId = default, IDictionary<string, object> parameters = default);

    string HtmlBody();
}