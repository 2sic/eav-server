using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.WebApi.Sys.Licenses
{
    public class LicenseControllerReal : WebApiBackendBase<LicenseControllerReal>, ILicenseController
    {
        public LicenseControllerReal(IServiceProvider serviceProvider, Lazy<ILicenseService> licenseServiceLazy, Lazy<IFeaturesInternal> featuresLazy) : base(serviceProvider, "Bck.Lics")
        {
            _licenseServiceLazy = licenseServiceLazy;
            _featuresLazy = featuresLazy;
        }
        private readonly Lazy<ILicenseService> _licenseServiceLazy;
        private readonly Lazy<IFeaturesInternal> _featuresLazy;

        /// <summary>
        /// Gives an array of License (sort by priority)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LicenseDto> Summary()
        {
            var licSer = _licenseServiceLazy.Value;
            var licenses = licSer.Catalog()
                .OrderBy(l => l.Priority);

            var features = _featuresLazy.Value.All;

            return licenses
                .Select(l => new LicenseDto
                {
                    Name = l.Name,
                    Priority = l.Priority,
                    Guid = l.Guid,
                    Description = l.Description,
                    AutoEnable = l.AutoEnable,
                    IsEnabled = licSer.IsEnabled(l),
                    Features = features
                        .Where(f => f.License == l.Name)
                        .OrderBy(f => f.NameId)
                });
        }
    }
}
