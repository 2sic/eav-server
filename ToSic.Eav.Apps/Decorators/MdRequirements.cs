using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps.Decorators
{
    public class MdRequirements: ReadBase
    {

        public MdRequirements(LazySvc<ILicenseService> licenseService, LazySvc<IFeaturesInternal> featsService, LazySvc<IPlatformInfo> platInfo, LicenseCatalog licenseCatalog)
            : base($"{AppConstants.LogName}.MdReq")
        {
            _licenseService = licenseService;
            _featsService = featsService;
            _platInfo = platInfo;
            _licenseCatalog = licenseCatalog;
        }
        private readonly LazySvc<ILicenseService> _licenseService;
        private readonly LazySvc<IFeaturesInternal> _featsService;
        private readonly LazySvc<IPlatformInfo> _platInfo;
        private readonly LicenseCatalog _licenseCatalog;


        public bool RequirementMet(IEnumerable<IEntity> requirement)
        {
            var entities = requirement?.ToList();
            var wrapLog = Log.Fn<bool>($"entities: {entities?.Count}");
            if (entities == null || !entities.Any()) return wrapLog.ReturnTrue("no data");
            var reqList = entities.OfType(RequirementDecorator.TypeName).ToList();
            if(!reqList.Any()) return wrapLog.ReturnTrue("no requirements");
            var result = reqList.All(RequirementMet);
            return wrapLog.ReturnAndLog(result);
        }

        public bool RequirementMet(IEntity requirement) => Log.Func(() =>
        {
            // No requirement, all is ok
            if (requirement == null) return (true, "no requirement");
            var reqObj = new RequirementDecorator(requirement);

            // Check requirement type
            switch (reqObj.RequirementType)
            {
                case RequirementDecorator.ReqFeature:
                    return (VerifyFeature(reqObj), "feature");
                case RequirementDecorator.ReqLicense:
                    return (VerifyLicense(reqObj), "license");
                case RequirementDecorator.ReqPlatform:
                    return (VerifyPlatform(reqObj), "platform");
                default:
                    // No real requirement, assume not fulfilled
                    return (false, "unknown requirement");
            }
        });

        private bool VerifyPlatform(RequirementDecorator reqObj) => Log.Func($"name: {reqObj.Platform}", () =>
        {
            if (string.IsNullOrWhiteSpace(reqObj.Platform)) return (true, "no req. platform");

            var enabled = _platInfo.Value.Name.EqualsInsensitive(reqObj.Platform.Trim());
            return (enabled, $"enabled: {enabled}");
        });

        private bool VerifyFeature(RequirementDecorator reqObj) => Log.Func($"name: {reqObj.Feature}", () =>
        {
            if (string.IsNullOrWhiteSpace(reqObj.Feature)) return (true, "no req. feature");

            var enabled = _featsService.Value.IsEnabled(reqObj.Feature.Trim());
            return (enabled, $"enabled: {enabled}");
        });

        private bool VerifyLicense(RequirementDecorator reqObj) => Log.Func($"name: {reqObj.License}", () =>
        {
            var wrapLog = Log.Fn<bool>($"name: {reqObj.License}");
            if (string.IsNullOrWhiteSpace(reqObj.License)) return (true, "no req. license");

            // find license
            var matchingLic = _licenseCatalog.TryGet(reqObj.License.Trim());
            if (matchingLic == null) return (false, "unknown license");

            var enabled = _licenseService.Value.IsEnabled(matchingLic);
            return (enabled, $"enabled {enabled}");
        });
    }
}
