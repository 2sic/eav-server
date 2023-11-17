using ToSic.Eav.Data;

namespace ToSic.Eav.SysData
{
    /// <summary>
    /// License entity - usually stored in the global / preset App
    /// </summary>
    internal class LicenseEntity: EntityBasedType
    {
        public static string TypeNameId = "57248ccb-24f1-44c6-9c6c-085e44ebb0cb";
        public static string ContentTypeName = "⚙️License";

        public LicenseEntity(IEntity entity) : base(entity)
        {
        }

        public string Fingerprint => GetThis("");

        public EnterpriseFingerprint AsEnterprise() => new EnterpriseFingerprint
        {
            Id = Id,
            Guid = Guid,
            Title = Title,
            Fingerprint = Fingerprint
        };
    }
}
