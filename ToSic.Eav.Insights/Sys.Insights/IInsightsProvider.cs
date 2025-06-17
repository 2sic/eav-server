namespace ToSic.Eav.Sys.Insights;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IInsightsProvider
{
    InsightsProviderSpecs Specs { get; }

    void SetContext(IInsightsLinker linker, int? appId, IDictionary<string, object?> parameters, string key, int? position, string type, bool? toggle, string nameId, string filter);

    string HtmlBody();
}

public record InsightsProviderSpecs
{
    public const string HiddenFromAutoDisplay = "Hidden";

    public string HelpCategory { get; init; } = HiddenFromAutoDisplay;
    public required string Name { get; init; }
    public string? Teaser { get; init; }

    [field: AllowNull, MaybeNull]
    public string Title { get => field ??= Name; init; }
}