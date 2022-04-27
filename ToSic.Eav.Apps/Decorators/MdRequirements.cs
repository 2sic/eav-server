using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Decorators
{
    public class MdRequirements: ReadBase<MdRequirements>
    {

        public MdRequirements(Lazy<ILicenseService> licenseService, Lazy<IFeaturesInternal> featsService, Lazy<IPlatformInfo> platInfo, LicenseCatalog licenseCatalog)
            : base($"{AppConstants.LogName}.MdReq")
        {
            _licenseService = licenseService;
            _featsService = featsService;
            _platInfo = platInfo;
            _licenseCatalog = licenseCatalog;
        }
        private readonly Lazy<ILicenseService> _licenseService;
        private readonly Lazy<IFeaturesInternal> _featsService;
        private readonly Lazy<IPlatformInfo> _platInfo;
        private readonly LicenseCatalog _licenseCatalog;


        public bool RequirementMet(IEnumerable<IEntity> requirement)
        {
            var entities = requirement?.ToList();
            var wrapLog = Log.Call<bool>($"entities: {entities?.Count}");
            if (entities == null || !entities.Any()) return wrapLog("no data", true);
            var reqList = entities.OfType(RequirementDecorator.TypeName).ToList();
            if(!reqList.Any()) return wrapLog("no requirements", true);
            var result = reqList.All(RequirementMet);
            return wrapLog($"{result}", result);
        }

        public bool RequirementMet(IEntity requirement)
        {
            var wrapLog = Log.Call<bool>();
            // No requirement, all is ok
            if (requirement == null) return wrapLog("no requirement", true);
            var reqObj = new RequirementDecorator(requirement);

            // Check requirement type
            switch (reqObj.RequirementType)
            {
                case RequirementDecorator.ReqFeature:
                    return wrapLog("feature", VerifyFeature(reqObj));
                case RequirementDecorator.ReqLicense:
                    return wrapLog("license", VerifyLicense(reqObj));
                case RequirementDecorator.ReqPlatform:
                    return wrapLog("platform", VerifyPlatform(reqObj));
                default:
                    // No real requirement, assume not fulfilled
                    return wrapLog("unknown requirement", false);
            }
        }

        private bool VerifyPlatform(RequirementDecorator reqObj)
        {
            var platName = reqObj.Platform;
            var wrapLog = Log.Call<bool>($"name: {platName}");
            if (string.IsNullOrWhiteSpace(platName)) return wrapLog("no req. platform", true);

            var enabled = _platInfo.Value.Name.EqualsInsensitive(platName.Trim());
            return wrapLog($"enabled: {enabled}", enabled);
        }

        private bool VerifyFeature(RequirementDecorator reqObj)
        {
            var featName = reqObj.Feature;
            var wrapLog = Log.Call<bool>($"name: {featName}");
            if (string.IsNullOrWhiteSpace(featName)) return wrapLog("no req. feature", true);

            var enabled = _featsService.Value.IsEnabled(featName.Trim());
            return wrapLog($"enabled: {enabled}", enabled);
        }

        private bool VerifyLicense(RequirementDecorator reqObj)
        {
            var licName = reqObj.License;
            var wrapLog = Log.Call<bool>($"name: {licName}");
            if (string.IsNullOrWhiteSpace(licName)) return wrapLog("no req. license", true);
            
            // find license
            var matchingLic = _licenseCatalog.TryGet(licName.Trim()); //  LicenseCatalog.Find(licName.Trim());
            if (matchingLic == null) return wrapLog("unknown license", false);
            
            var enabled = _licenseService.Value.IsEnabled(matchingLic);
            return wrapLog($"enabled {enabled}", enabled);
        }
    }
}
