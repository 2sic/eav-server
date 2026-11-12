using ToSic.Sys.Data;

namespace ToSic.Eav.Ui.Edit.Sys.Load;

/// <summary>
/// API Keys to use in the UI - such as Google Maps, Google Translate etc.
/// </summary>
public record ContextApiKeyDto : IHasIdentityNameId
{
    public required string NameId { get; init; }

    public required string ApiKey { get; init; }

    public required bool IsDemo { get; init; }
}