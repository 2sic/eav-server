namespace ToSic.Eav.WebApi.Sys.Licenses;

public class LicenseDto
{
    public required string Name { get; init; }
    public required int Priority { get; init; }
    public required Guid Guid { get; init; }
    public required string Description { get; init; }

    public required bool AutoEnable { get; init; }
    public required bool IsEnabled { get; init; }

    public required DateTime? Expires { get; init; }

    public required ICollection<FeatureStateDto> Features { get; init; }
}