namespace ToSic.Eav.WebApi.Sys.Licenses;

public class LicenseDto
{
    public string Name { get; init; }
    public int Priority { get; init; }
    public Guid Guid { get; init; }
    public string Description { get; init; }

    public bool AutoEnable { get; init; }
    public bool IsEnabled { get; init; }

    public DateTime? Expires { get; init; }

    public ICollection<FeatureStateDto> Features { get; init; }
}