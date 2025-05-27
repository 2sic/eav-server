using ToSic.Eav.Context;
using ToSic.Eav.Internal.Requirements;
using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;
using ToSic.Sys.Utils;
using static ToSic.Eav.Apps.Internal.MetadataDecorators.RequirementDecorator;
using SysFeaturesService = ToSic.Sys.Capabilities.SysFeatures.SysFeaturesService;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MdRequirements(
    LazySvc<ILicenseService> licenseService,
    LazySvc<ISysFeaturesService> featsService,
    LazySvc<IPlatformInfo> platInfo,
    LicenseCatalog licenseCatalog,
    LazySvc<SysFeaturesService> sysCapSvc)
    : ServiceBase($"{AppConstants.LogName}.MdReq",
        connect: [licenseService, featsService, platInfo, licenseCatalog, sysCapSvc]), IRequirementsService
{
    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements)
    {
        var list = requirements?.ToList();
        var l = Log.Fn<List<RequirementStatus>>();
        if (list.SafeNone())
            return l.Return([], "empty requirements");

        var reqStatus = list
            .Select(r => VerifySysCap(r.NameId))
            .Where(pair => !pair.IsOk)
            .Select(pair => new RequirementStatus(false, pair.Aspect, ""))
            .ToList();
        return l.Return(reqStatus, $"not ok count: {reqStatus.Count}");
    }

    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements)
    {
        var l = Log.Fn<List<RequirementStatus>>();

        var (ok, notOk) = CheckRequirements(requirements);
        return ok 
            ? l.Return([], "all ok")
            : l.Return(notOk.Cast<RequirementStatus>().ToList(), $"a few not ok: {notOk.Count}");
    }

    public (bool Approved, string FeatureId) RequirementMet(IEnumerable<IEntity> requirement)
    {
        var l = Log.Fn<(bool, string)>();

        var (ok, notOk) = CheckRequirements(requirement);
        if (ok)
            return l.Return((true, ""), "all ok");

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

        return reqStatus.All(rs => rs.IsOk)
            ? l.Return((true, null), "all ok")
            : l.Return((false, reqStatus.Where(r => !r.IsOk).ToList()), "some didn't work");
    }

    private record ReqStatusPrivate(RequirementDecorator Decorator, string NameId, bool Approved, Aspect Aspect = default)
        : RequirementStatus(Approved, Aspect ?? Aspect.None with { NameId = NameId }, "")
    {
        //public RequirementDecorator Decorator = decorator;
    }

    private ReqStatusPrivate RequirementMet(IEntity requirement)
    {
        var l = Log.Fn<ReqStatusPrivate>();
        // No requirement, all is ok
        if (requirement == null)
            return l.Return(new(null, ReqNone, true), ReqNone);
        var reqDec = new RequirementDecorator(requirement);

        // Check requirement type
        return reqDec.RequirementType switch
        {
            ReqFeature => BuildAndRet(VerifyFeature(reqDec), reqDec.Feature?.Trim(), ReqFeature),
            ReqLicense => BuildAndRet(VerifyLicense(reqDec), reqDec.License?.Trim(), ReqLicense),
            ReqPlatform => BuildAndRet(VerifyPlatform(reqDec), reqDec.Platform?.Trim(), ReqPlatform),
            ReqSysCap => BuildAndRet(VerifySysCap(reqDec), reqDec.SystemCapability?.Trim(), ReqSysCap),
            _ => BuildAndRet((false, Aspect.None), ReqUnknown, ReqUnknown)
        };

        ReqStatusPrivate BuildAndRet((bool approved, Aspect aspect) check, string nameId, string reason) 
            => l.Return(new(reqDec, nameId, check.approved, check.aspect), reason);
    }

    private (bool IsOk, Aspect Aspect) VerifyPlatform(RequirementDecorator reqObj)
        => VerifyPlatform(reqObj.Platform?.Trim());

    private (bool IsOk, Aspect Aspect) VerifyPlatform(string platform)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {platform}");
        if (platform.IsEmptyOrWs())
            return l.Return((true, Aspect.None), "no req. platform");

        var enabled = platInfo.Value.Name.EqualsInsensitive(platform);
        return l.Return((enabled, Aspect.Custom(platform, Guid.Empty, platform)), $"enabled: {enabled}");
    }


    private (bool IsOk, Aspect Aspect) VerifyFeature(RequirementDecorator reqObj)
        => VerifyFeature(reqObj.Feature?.Trim());

    private (bool IsOk, Aspect Aspect) VerifyFeature(string feat)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {feat}");
        if (feat.IsEmptyOrWs())
            return l.Return((true, Aspect.None), "no req. feature");

        var enabled = featsService.Value.IsEnabled(feat);
        var status = featsService.Value.Get(feat);
        return l.Return((enabled, status?.Aspect ?? Aspect.None), $"enabled: {enabled}");
    }


    private (bool IsOk, Aspect Aspect) VerifySysCap(RequirementDecorator reqObj)
        => VerifySysCap(reqObj.SystemCapability?.Trim());

    private (bool IsOk, Aspect Aspect) VerifySysCap(string sysCap)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {sysCap}");
        if (sysCap.IsEmptyOrWs())
            return l.Return((true, Aspect.None), "no req. feature");

        var enabled = sysCapSvc.Value.IsEnabled(sysCap);
        var status = sysCapSvc.Value.GetDef(sysCap);
        return l.Return((enabled, status ?? Aspect.None), $"enabled: {enabled}");
    }


    private (bool IsOk, Aspect Aspect) VerifyLicense(RequirementDecorator reqObj)
        => VerifyLicense(reqObj.License?.Trim());

    private (bool IsOk, Aspect Aspect) VerifyLicense(string license)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {license}");
        if (license.IsEmptyOrWs())
            return l.Return((true, Aspect.None), "no req. license");

        // find license
        var matchingLic = licenseCatalog.TryGet(license);
        if (matchingLic == null)
            return l.Return((false, GenAspectFromLicense()), "unknown license");

        var enabled = licenseService.Value.IsEnabled(matchingLic);
        return l.Return((enabled, GenAspectFromLicense()), $"enabled {enabled}");

        Aspect GenAspectFromLicense() => Aspect.Custom(
            license,
            Guid.TryParse(license, out var lic) ? lic : Guid.Empty
        );
    }
}