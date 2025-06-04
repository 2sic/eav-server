using ToSic.Eav.Data.EntityBased.Sys;
using ToSic.Sys.Capabilities.Fingerprints;

namespace ToSic.Eav.Configuration.Sys.Loaders;

/// <summary>
/// License entity - usually stored in the global / preset App.
/// Used by the FeaturesLoader to load the license information from inside JSON entities.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class LicenseEntity(IEntity entity) : EntityBasedType(entity)
{
    public static string TypeNameId = "57248ccb-24f1-44c6-9c6c-085e44ebb0cb";
    public static string ContentTypeName = "⚙️License";

    public string Fingerprint => GetThis("");

    public EnterpriseFingerprint AsEnterprise() => new()
    {
        Id = Id,
        Guid = Guid,
        Title = Title,
        Fingerprint = Fingerprint
    };
}