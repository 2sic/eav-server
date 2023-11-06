using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static ToSic.Eav.Apps.Decorators.RequirementDecorator;

namespace ToSic.Eav.Apps.Decorators
{
    public class MdRequirements: ServiceBase
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

        public (bool Approved, string FeatureId) RequirementMet(IEnumerable<IEntity> requirement)
        {
            var l = Log.Fn<(bool, string)>();
            var entities = requirement?.ToList();
            l.A($"entities: {entities?.Count}");
            if (entities == null || !entities.Any()) return l.Return((true, ""), "no metadata");
            var reqList = entities.OfType(TypeName).ToList();
            if (!reqList.Any()) return l.Return((true, ""), "no requirements");
            var reqStatus = reqList.Select(r => new
            {
                Entity = r,
                Status = RequirementMet(r)
            }).ToList();
            var result = reqStatus.All(rs => rs.Status.Approved);
            if (result) return l.Return((true, ""), "all ok");

            // If false, check if it's only a feature that's missing
            var notOk = reqStatus.Where(rs => !rs.Status.Approved).ToList();
            var allFeatures = notOk.Count(rs => rs.Status.Decorator.RequirementType == ReqFeature) == notOk.Count;
            if (!allFeatures || notOk.Count > 1)
                return l.Return((false, ""), "not ok, but not just because of a single features");

            var featureName = notOk.First().Status.Decorator.Feature;
            return l.Return((false, featureName), $"not ok, because of feature {featureName}");

            // return ((result, ""), result ? "ok" : "not ok");
        }

        internal (bool Approved, RequirementDecorator Decorator) RequirementMet(IEntity requirement)
        {
            var l = Log.Fn<(bool, RequirementDecorator)>();
            // No requirement, all is ok
            if (requirement == null) return l.Return((true, null), "no requirement");
            var reqObj = new RequirementDecorator(requirement);

            // Check requirement type
            switch (reqObj.RequirementType)
            {
                case ReqFeature:
                    return l.Return((VerifyFeature(reqObj), reqObj), "feature");
                case ReqLicense:
                    return l.Return((VerifyLicense(reqObj), reqObj), "license");
                case ReqPlatform:
                    return l.Return((VerifyPlatform(reqObj), reqObj), "platform");
                default:
                    // No known requirement, assume not fulfilled
                    return l.Return((false, null), "unknown requirement");
            }
        }

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
            if (string.IsNullOrWhiteSpace(reqObj.License)) return (true, "no req. license");

            // find license
            var matchingLic = _licenseCatalog.TryGet(reqObj.License.Trim());
            if (matchingLic == null) return (false, "unknown license");

            var enabled = _licenseService.Value.IsEnabled(matchingLic);
            return (enabled, $"enabled {enabled}");
        });
    }
}
