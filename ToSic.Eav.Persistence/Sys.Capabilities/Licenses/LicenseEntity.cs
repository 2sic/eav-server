using ToSic.Eav.Model;
using ToSic.Sys.Capabilities.Fingerprints;

namespace ToSic.Sys.Capabilities.Licenses;

/// <summary>
/// License entity - usually stored in the global / preset App.
/// Used by the FeaturesLoader to load the license information from inside JSON entities.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
internal record LicenseEntity : ModelOfEntity
{
    public static string TypeNameId = "57248ccb-24f1-44c6-9c6c-085e44ebb0cb";
    public static string ContentTypeName = "⚙️License";

    public string Fingerprint => GetThis("");
}

internal static class LicenseEntityExtensions
{
    public static EnterpriseFingerprint AsEnterprise(this LicenseEntity entity) => new()
    {
        Id = entity.Id,
        Guid = entity.Guid,
        Title = entity.Title,
        Fingerprint = entity.Fingerprint
    };
}