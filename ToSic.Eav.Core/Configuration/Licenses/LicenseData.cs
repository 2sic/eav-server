using ToSic.Eav.Data;

namespace ToSic.Eav.Configuration.Licenses
{
    internal class LicenseData: EntityBasedType
    {
        public static string TypeNameId = "57248ccb-24f1-44c6-9c6c-085e44ebb0cb";

        public LicenseData(IEntity entity) : base(entity)
        {
        }

        public string Fingerprint => Get(nameof(Fingerprint), "");
    }
}
