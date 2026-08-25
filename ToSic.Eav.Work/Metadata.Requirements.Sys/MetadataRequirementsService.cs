using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Models;
using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Capabilities.Platform;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;
using ToSic.Sys.Utils;
using static ToSic.Sys.Capabilities.FeatureConstants;

namespace ToSic.Eav.Metadata.Requirements.Sys;

// TODO: This should be replaced with the RequirementsService.
// Historically we first had this
// Then we started to create the RequirementsService
//
// But as of 2026-08-21 the requirements service does not yet have
// - licenses check
// - Platform check
// - Wish: Permissions requirement!
//
// Once these have been added, this service should be removed/replaced;
// It would then also be relevant to possibly change how the metadata is saved;
// As of now, Metadata-Requirements are probably only used internally.

/// <summary>
/// Provides requirements from the metadata of anything.
/// </summary>
/// <remarks>
/// As of 2026-08-25 the data in 2sxc 3 checks for features; 1 for licenses; others currently not in use as of now.
///
/// From what I can tell (2dm) it's currently only used to provide warnings in the UI for editors.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MetadataRequirementsService(
    LazySvc<ILicenseService> licenseService,
    LazySvc<ISysFeaturesService> featsService,
    LazySvc<IPlatformInfo> platInfo,
    LicenseCatalog licenseCatalog
) : ServiceBase($"{AppConstants.LogName}.MdReq",
        connect: [licenseService, featsService, platInfo, licenseCatalog]), IRequirementsService
{
    public ICollection<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements)
    {
        var list = requirements?.ToListOpt();
        var l = Log.Fn<ICollection<RequirementStatus>>();
        if (list.SafeNone())
            return l.Return([], "empty requirements");

        var reqStatus = list
            .Select(r => VerifyFeature(r.NameId))
            .Where(pair => !pair.IsOk)
            .Select(pair => new RequirementStatus(false, new(RequirementSysCapability, pair.Aspect.NameId), pair.Aspect, ""))
            .ToListOpt();
        return l.Return(reqStatus, $"not ok count: {reqStatus.Count}");
    }

    public ICollection<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements)
    {
        var l = Log.Fn<ICollection<RequirementStatus>>();

        var (ok, notOk) = CheckRequirements(requirements);
        return ok 
            ? l.Return([], "all ok")
            : l.Return(notOk.Cast<RequirementStatus>().ToListOpt(), $"a few not ok: {notOk.Count}");
    }

    public (bool Approved, string FeatureId) RequirementMet(IEnumerable<IEntity> requirement)
    {
        var l = Log.Fn<(bool, string)>();

        var (ok, notOk) = CheckRequirements(requirement);
        if (ok)
            return l.Return((true, ""), "all ok");

        // If false, check if it's only a feature that's missing
        var allFeatures = notOk.Count(rs => rs.Decorator.RequirementType == RequirementFeature) == notOk.Count;
        if (!allFeatures || notOk.Count > 1)
            return l.Return((false, ""), "not ok, but not just because of a single features");

        var featureName = notOk.First().Decorator.Feature;
        return l.Return((false, featureName), $"not ok, because of feature {featureName}");
    }

    private (bool AllOk, ICollection<ReqStatusWithDecorator> Issues) CheckRequirements(IEnumerable<IEntity> requirement)
    {
        var l = Log.Fn<(bool, ICollection<ReqStatusWithDecorator>)>();
        var entities = requirement?.ToListOpt();
        l.A($"entities: {entities?.Count}");
        if (entities == null || !entities.Any())
            return l.Return((true, []), "no metadata");

        // Preflight - ensure that they are of type RequirementDecorator
        var reqList = entities.GetModels<RequirementDecorator>().ToListOpt();
        if (!reqList.Any())
            return l.Return((true, []), "no requirements");

        var reqStatus = reqList
            .Select(RequirementMet)
            .OfType<ReqStatusWithDecorator>()
            .ToListOpt();

        return reqStatus.All(rs => rs.IsOk)
            ? l.Return((true, []), "all ok")
            : l.Return((false, reqStatus.Where(r => !r.IsOk).ToListOpt()), "some didn't work");
    }

    private record ReqStatusWithDecorator : RequirementStatus
    {
        public ReqStatusWithDecorator(RequirementDecorator decorator, string nameId, bool approved, Requirement requirement, Aspect? aspect = default)
            : base(approved, requirement, aspect ?? Aspect.UnknownAspect(decorator.RequirementType, nameId), "")
        {
            Decorator = decorator;
        }

        public RequirementDecorator Decorator { get; }

    }

    private ReqStatusWithDecorator? RequirementMet(RequirementDecorator? requirement)
    {
        var l = Log.Fn<ReqStatusWithDecorator>();
        // No requirement, all is ok
        if (requirement == null)
            return l.ReturnNull();
        var reqDec = requirement;

        // Check requirement type
        return reqDec.RequirementType switch
        {
            RequirementFeature => BuildAndRet(VerifyFeature(reqDec), reqDec.Feature.Trim(), RequirementFeature),
            ReqLicense => BuildAndRet(VerifyLicense(reqDec), reqDec.License.Trim(), ReqLicense),
            ReqPlatform => BuildAndRet(VerifyPlatform(reqDec), reqDec.Platform.Trim(), ReqPlatform),
            RequirementSysCapability => BuildAndRet(VerifySysCap(reqDec), reqDec.SystemCapability.Trim(), RequirementSysCapability),
            _ => BuildAndRet((false, Aspect.UnknownChecker(reqDec.RequirementType)), ReqUnknown, ReqUnknown)
        };

        ReqStatusWithDecorator BuildAndRet((bool approved, Aspect aspect) check, string nameId, string type) 
            => l.Return(new(reqDec, nameId, check.approved, new(type, nameId), check.aspect), type);
    }

    private (bool IsOk, Aspect Aspect) VerifyPlatform(RequirementDecorator reqObj)
        => VerifyPlatform(reqObj.Platform.Trim());

    private (bool IsOk, Aspect Aspect) VerifyPlatform(string? platform)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {platform}");
        if (platform.IsEmptyOrWs())
            return l.Return((true, Aspect.EmptyAspect(ReqPlatform)), "no req. platform");

        var enabled = platInfo.Value.Name.EqualsInsensitive(platform);
        return l.Return((enabled, Aspect.Custom(platform, Guid.Empty, platform)), $"enabled: {enabled}");
    }


    private (bool IsOk, Aspect Aspect) VerifyFeature(RequirementDecorator reqObj)
        => VerifyFeature(reqObj.Feature.Trim());

    private (bool IsOk, Aspect Aspect) VerifyFeature(string? feat)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {feat}");
        if (feat.IsEmptyOrWs())
            return l.Return((true, Aspect.EmptyAspect(RequirementFeature)), "no req. feature");

        var enabled = featsService.Value.IsEnabled(feat);
        var status = featsService.Value.Get(feat);
        return l.Return((enabled, status?.Aspect ?? Aspect.UnknownAspect(RequirementFeature, feat)), $"enabled: {enabled}");
    }


    private (bool IsOk, Aspect Aspect) VerifySysCap(RequirementDecorator reqObj)
        => VerifyFeature(reqObj.SystemCapability.Trim());



    private (bool IsOk, Aspect Aspect) VerifyLicense(RequirementDecorator reqObj)
        => VerifyLicense(reqObj.License.Trim());

    private (bool IsOk, Aspect Aspect) VerifyLicense(string? license)
    {
        var l = Log.Fn<(bool IsEnabled, Aspect Aspect)>($"name: {license}");
        if (license.IsEmptyOrWs())
            return l.Return((true, Aspect.EmptyAspect(ReqLicense)), "no req. license");

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