namespace ToSic.Eav.Configuration.Licenses
{
    public static class LicenseServiceExtensions
    {
        public static void ThrowIfNotLicensed(this ILicenseService licSer, LicenseDefinition lic)
        {
            if (licSer.IsEnabled(lic)) return;
            throw new LicenseDisabledException(lic);
        }
    }
}
