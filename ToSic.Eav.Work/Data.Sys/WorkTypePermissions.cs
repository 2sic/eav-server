using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Eav.Context;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Data.Sys;

/// <summary>
/// Quick Permission Check - will throw if not explicitly allowed.
/// </summary>
/// <param name="ctxSvc"></param>
/// <param name="typePermissions"></param>
[PrivateApi]
public class WorkTypePermissions(IContextOfSiteService ctxSvc, Generator<MultiPermissionsTypes, MultiPermissionsTypes.Options> typePermissions)
    : ServiceWithSetup<IAppWorkContext>("Wrk.EntAdm", connect: [ctxSvc, typePermissions])
{
    public void ThrowIfNotAllowed(string? contentType, List<Grants> grants)
        => GetDraftPermissionAndThrowIfNotAllowed(contentType, grants);

    public bool GetDraftPermissionAndThrowIfNotAllowed(string? contentType, List<Grants> grants)
    {
        var permCheck = typePermissions.New(new()
        {
            SiteContext = ctxSvc.Context,
            App = MyOptions,
            ContentTypes = [contentType.NullIfNoValue() ?? "MadeUpTypeForEmergenciesShouldNotBeAllowed"]
        });
        if (permCheck.EnsureAll(grants) is { Allowed: false } result)
            throw new PermissionDeniedException(result.ErrorMessage);
        
        return permCheck.EnsureAnyReadDraft().Allowed;
    }
}
