using System.Collections.Generic;
using System.Collections.Immutable;

namespace ToSic.Eav.Configuration.Licenses
{
    public interface ILicenseService
    {
        /// <summary>
        /// All licenses
        /// </summary>
        List<LicenseState> All { get; }

        /// <summary>
        /// Enabled licenses, in a dictionary to retrieve with the LicenseDefinition object
        /// </summary>
        /// <remarks>
        /// We use the real static LicenseDefinition as an index, because this ensures that people can't inject other license objects to bypass security.
        /// </remarks>
        IImmutableDictionary<LicenseDefinition, LicenseState> Enabled { get; }

        /// <summary>
        /// Check if a license is enabled - using the real primary LicenseDefinition object as the key.
        /// </summary>
        /// <returns></returns>
        bool IsEnabled(LicenseDefinition license);
    }
}