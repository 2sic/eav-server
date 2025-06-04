using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.WebApi.Sys.Security;

internal static class PublicFormsPermissions
{
    internal static bool UserCanWriteAndPublicFormsEnabled(this MultiPermissionsApp mpa, out HttpExceptionAbstraction preparedException, out string error)
    {
        var l = mpa.Log.Fn<bool>("");
        // 1. check if user is restricted
        var userIsRestricted = !mpa.UserMayOnAll(GrantSets.WritePublished);

        // 2. check if feature is enabled
        var feats = new[] { BuiltInFeatures.PublicEditForm.Guid };
        var sysFeatures = mpa.FeaturesInternal;
        if (userIsRestricted && !sysFeatures.IsEnabled(feats))
        {
            error = $"low-permission users may not access this - {sysFeatures.MsgMissingSome(feats)}";
            preparedException = HttpException.PermissionDenied(error);
            return l.ReturnFalse();
        }
        preparedException = null;
        error = null;
        return l.ReturnTrue("ok");
    }

}