using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.WebApi.Licenses
{
    public class LicenseBackend : WebApiBackendBase<LicenseBackend>
    {

        #region Constructor / DI

        public LicenseBackend(IServiceProvider serviceProvider, Lazy<ILicenseService> licenseServiceLazy, Lazy<IFeaturesInternal> featuresLazy) : base(serviceProvider, "Bck.Lics")
        {
            _licenseServiceLazy = licenseServiceLazy;
            _featuresLazy = featuresLazy;
        }

        private readonly Lazy<ILicenseService> _licenseServiceLazy;
        private readonly Lazy<IFeaturesInternal> _featuresLazy;

        #endregion

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
                    Features = features.Where(f => f.License == l.Name)
                });
        }
    }
}
