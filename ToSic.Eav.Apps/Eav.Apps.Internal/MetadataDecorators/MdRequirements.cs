using ToSic.Eav.Context;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.Plumbing;
using ToSic.Eav.SysData;
using static ToSic.Eav.Apps.Internal.MetadataDecorators.RequirementDecorator;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MdRequirements: ServiceBase, IRequirementsService
{

    public MdRequirements(LazySvc<ILicenseService> licenseService, LazySvc<IEavFeaturesService> featsService, LazySvc<IPlatformInfo> platInfo, LicenseCatalog licenseCatalog, LazySvc<SysFeaturesService> sysCapSvc)
        : base($"{AppConstants.LogName}.MdReq")
    {
        ConnectServices(
            _licenseService = licenseService,
            _featsService = featsService,
            _platInfo = platInfo,
            _licenseCatalog = licenseCatalog,
            _sysCapSvc = sysCapSvc
        );
    }
    private readonly LazySvc<ILicenseService> _licenseService;
    private readonly LazySvc<IEavFeaturesService> _featsService;
    private readonly LazySvc<IPlatformInfo> _platInfo;
    private readonly LicenseCatalog _licenseCatalog;
    private readonly LazySvc<SysFeaturesService> _sysCapSvc;

    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements)
    {
        var l = Log.Fn<List<RequirementStatus>>();

        var (ok, notOk) = CheckRequirements(requirements);
        return ok 
            ? l.Return(new List<RequirementStatus>(), "all ok")
            : l.Return(notOk.Cast<RequirementStatus>().ToList(), $"a few not ok: {notOk.Count}");
    }

    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements)
    {
        var l = Log.Fn<List<RequirementStatus>>();
        if (requirements.SafeNone()) return l.Return(new List<RequirementStatus>());

        var reqStatus = requirements
            .Select(r => VerifySysCap(r.NameId))
            .Where(pair => !pair.IsEnabled)
            .Select(pair => new RequirementStatus(false, pair.Aspect))
            .ToList();
        return l.Return(reqStatus, $"not ok count: {reqStatus.Count}");
    }

    public (bool Approved, string FeatureId) RequirementMet(IEnumerable<IEntity> requirement)
    {
        var l = Log.Fn<(bool, string)>();

        var (ok, notOk) = CheckRequirements(requirement);
        if (ok) return l.Return((true, ""), "all ok");

        // If false, check if it's only a feature that's missing
        var allFeatures = notOk.Count(rs => rs.Decorator.RequirementType == ReqFeature) == notOk.Count;
        if (!allFeatures || notOk.Count > 1)
            return l.Return((false, ""), "not ok, but not just because of a single features");

        var featureName = notOk.First().Decorator.Feature;
        return l.Return((false, featureName), $"not ok, because of feature {featureName}");
    }

    private (bool AllOk, List<ReqStatusPrivate> Issues) CheckRequirements(IEnumerable<IEntity> requirement)
    {
        var l = Log.Fn<(bool, List<ReqStatusPrivate>)>();
        var entities = requirement?.ToList();
        l.A($"entities: {entities?.Count}");
        if (entities == null || !entities.Any())
            return l.Return((true, null), "no metadata");

        // Preflight - ensure that they are of type RequirementDecorator
        var reqList = entities.OfType(TypeName).ToList();
        if (!reqList.Any())
            return l.Return((true, null), "no requirements");

        var reqStatus = reqList
            .Select(RequirementMet)
            .ToList();

        if (reqStatus.All(rs => rs.IsOk))
            return l.Return((true, null), "all ok");

        return l.Return((false, reqStatus.Where(r => !r.IsOk).ToList()), "some didn't work");
    }

    internal class ReqStatusPrivate(RequirementDecorator decorator, string nameId, bool approved, Aspect aspect = default)
        : RequirementStatus(approved, aspect ?? SysData.Aspect.None.Clone(nameId: nameId))
    {
        public RequirementDecorator Decorator = decorator;
    }

    internal ReqStatusPrivate RequirementMet(IEntity requirement)
    {
        var l = Log.Fn<ReqStatusPrivate>();
        // No requirement, all is ok
        if (requirement == null) return l.Return(new ReqStatusPrivate(null, ReqNone, true), ReqNone);
        var reqObj = new RequirementDecorator(requirement);

        ReqStatusPrivate BuildAndRet((bool approved, Aspect aspect) check, string nameId, string reason) 
            => l.Return(new ReqStatusPrivate(reqObj, nameId, check.approved, check.aspect), reason);

        // Check requirement type
        switch (reqObj.RequirementType)
        {
            case ReqFeature: return BuildAndRet(VerifyFeature(reqObj), reqObj.Feature?.Trim(), ReqFeature);
            case ReqLicense: return BuildAndRet(VerifyLicense(reqObj), reqObj.License?.Trim(), ReqLicense);
            case ReqPlatform: return BuildAndRet(VerifyPlatform(reqObj), reqObj.Platform?.Trim(), ReqPlatform);
            case ReqSysCap: return BuildAndRet(VerifySysCap(reqObj), reqObj.SystemCapability?.Trim(), ReqSysCap);
            // No known requirement, assume not fulfilled
            default: return  BuildAndRet((false, Aspect.None), ReqUnknown, ReqUnknown);
        }
    }

    private (bool IsEnabled, Aspect Aspect) VerifyPlatform(RequirementDecorator reqObj) => VerifyPlatform(reqObj.Platform?.Trim());

    private (bool IsEnabled, Aspect Aspect) VerifyPlatform(string platform)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {platform}");
        if (platform.IsEmptyOrWs()) return l.Return((true, Aspect.None), "no req. platform");

        var enabled = _platInfo.Value.Name.EqualsInsensitive(platform);
        return l.Return((enabled, Aspect.Custom(platform, Guid.Empty, platform)), $"enabled: {enabled}");
    }


    private (bool IsEnabled, Aspect Aspect) VerifyFeature(RequirementDecorator reqObj) => VerifyFeature(reqObj.Feature?.Trim());

    private (bool IsEnabled, Aspect Aspect) VerifyFeature(string feat)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {feat}");
        if (feat.IsEmptyOrWs()) return l.Return((true, Aspect.None), "no req. feature");

        var enabled = _featsService.Value.IsEnabled(feat);
        var status = _featsService.Value.Get(feat);
        return l.Return((enabled, status?.Aspect ?? Aspect.None), $"enabled: {enabled}");
    }


    private (bool IsEnabled, Aspect Aspect) VerifySysCap(RequirementDecorator reqObj) => VerifySysCap(reqObj.SystemCapability?.Trim());

    private (bool IsEnabled, Aspect Aspect) VerifySysCap(string sysCap)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {sysCap}");
        if (sysCap.IsEmptyOrWs()) return l.Return((true, Aspect.None), "no req. feature");

        var enabled = _sysCapSvc.Value.IsEnabled(sysCap);
        var status = _sysCapSvc.Value.GetDef(sysCap);
        return l.Return((enabled, status ?? Aspect.None), $"enabled: {enabled}");
    }


    private (bool IsEnabled, Aspect Aspect) VerifyLicense(RequirementDecorator reqObj) => VerifyLicense(reqObj.License?.Trim());

    private (bool IsEnabled, Aspect Aspect) VerifyLicense(string license)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {license}");
        if (license.IsEmptyOrWs()) return l.Return((true, Aspect.None), "no req. license");

        Aspect GenAspectFromLicense() => Aspect.Custom(license, Guid.TryParse(license, out var lic) ? lic : Guid.Empty);

        // find license
        var matchingLic = _licenseCatalog.TryGet(license);
        if (matchingLic == null) return l.Return((false, GenAspectFromLicense()), "unknown license");

        var enabled = _licenseService.Value.IsEnabled(matchingLic);
        return l.Return((enabled, GenAspectFromLicense()), $"enabled {enabled}");
    }
}