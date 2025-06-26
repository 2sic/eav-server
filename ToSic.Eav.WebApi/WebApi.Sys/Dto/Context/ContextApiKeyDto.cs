using ToSic.Lib.Data;

namespace ToSic.Eav.WebApi.Sys.Dto;

/// <summary>
/// API Keys to use in the UI - such as Google Maps, Google Translate etc.
/// </summary>
public class ContextApiKeyDto : IHasIdentityNameId
{
    public required string NameId { get; init; }

    public required string ApiKey { get; init; }

    public required bool IsDemo { get; init; }
}