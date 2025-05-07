using ToSic.Eav.Data;
using ToSic.Lib.Sys.Fingerprints.Internal;

namespace ToSic.Eav.SysData;

/// <summary>
/// License entity - usually stored in the global / preset App
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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