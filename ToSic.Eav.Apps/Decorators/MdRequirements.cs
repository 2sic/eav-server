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

        public MdRequirements(LazyInit<ILicenseService> licenseService, LazyInit<IFeaturesInternal> featsService, LazyInit<IPlatformInfo> platInfo, LicenseCatalog licenseCatalog)
            : base($"{AppConstants.LogName}.MdReq")
        {
            _licenseService = licenseService;
            _featsService = featsService;
            _platInfo = platInfo;
            _licenseCatalog = licenseCatalog;
        }
        private readonly LazyInit<ILicenseService> _licenseService;
        private readonly LazyInit<IFeaturesInternal> _featsService;
        private readonly LazyInit<IPlatformInfo> _platInfo;
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

        public bool RequirementMet(IEntity requirement)
        {
            var wrapLog = Log.Fn<bool>();
            // No requirement, all is ok
            if (requirement == null) return wrapLog.ReturnTrue("no requirement");
            var reqObj = new RequirementDecorator(requirement);

            // Check requirement type
            switch (reqObj.RequirementType)
            {
                case RequirementDecorator.ReqFeature:
                    return wrapLog.Return(VerifyFeature(reqObj), "feature");
                case RequirementDecorator.ReqLicense:
                    return wrapLog.Return(VerifyLicense(reqObj), "license");
                case RequirementDecorator.ReqPlatform:
                    return wrapLog.Return(VerifyPlatform(reqObj), "platform");
                default:
                    // No real requirement, assume not fulfilled
                    return wrapLog.ReturnFalse("unknown requirement");
            }
        }

        private bool VerifyPlatform(RequirementDecorator reqObj)
        {
            var platName = reqObj.Platform;
            var wrapLog = Log.Fn<bool>($"name: {platName}");
            if (string.IsNullOrWhiteSpace(platName)) return wrapLog.ReturnTrue("no req. platform");

            var enabled = _platInfo.Value.Name.EqualsInsensitive(platName.Trim());
            return wrapLog.Return(enabled, $"enabled: {enabled}");
        }

        private bool VerifyFeature(RequirementDecorator reqObj)
        {
            var featName = reqObj.Feature;
            var wrapLog = Log.Fn<bool>($"name: {featName}");
            if (string.IsNullOrWhiteSpace(featName)) return wrapLog.ReturnTrue("no req. feature");

            var enabled = _featsService.Value.IsEnabled(featName.Trim());
            return wrapLog.Return(enabled, $"enabled: {enabled}");
        }
        
        private bool VerifyLicense(RequirementDecorator reqObj)
        {
            var licName = reqObj.License;
            var wrapLog = Log.Fn<bool>($"name: {licName}");
            if (string.IsNullOrWhiteSpace(licName)) return wrapLog.ReturnTrue("no req. license");
            
            // find license
            var matchingLic = _licenseCatalog.TryGet(licName.Trim()); //  LicenseCatalog.Find(licName.Trim());
            if (matchingLic == null) return wrapLog.ReturnFalse("unknown license");
            
            var enabled = _licenseService.Value.IsEnabled(matchingLic);
            return wrapLog.Return(enabled, $"enabled {enabled}");
        }
    }
}
