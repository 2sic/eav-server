using ToSic.Eav.Apps.Security;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.Logging;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Errors;

namespace ToSic.Eav.WebApi.Security;

internal static class PublicFormsPermissions
{
    internal static bool UserCanWriteAndPublicFormsEnabled(this MultiPermissionsApp mpa, out HttpExceptionAbstraction preparedException, out string error)
    {
        var wrapLog = mpa.Log.Fn<bool>("");
        // 1. check if user is restricted
        var userIsRestricted = !mpa.UserMayOnAll(GrantSets.WritePublished);

        // 2. check if feature is enabled
        var feats = new[] { BuiltInFeatures.PublicEditForm.Guid };
        var sysFeatures = mpa.FeaturesInternal;
        if (userIsRestricted && !sysFeatures.IsEnabled(feats))
        {
            error = $"low-permission users may not access this - {sysFeatures.MsgMissingSome(feats)}";
            preparedException = HttpException.PermissionDenied(error);
            return wrapLog.ReturnFalse();
        }
        preparedException = null;
        error = null;
        return wrapLog.ReturnTrue("ok");
    }

}