using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.WebApi.Sys.Security;

internal static class PublicFormsPermissions
{
    internal static bool UserCanWriteAndPublicFormsEnabled(this MultiPermissionsTypes mpa, LazySvc<ISysFeaturesService> sysFeatSvc, [NotNullWhen(false)] out Exception? preparedException)
    {
        var l = mpa.Log.Fn<bool>("");
        // 1. check if user is restricted
        var userIsRestricted = !mpa.EnsureAll(GrantSets.WritePublished).Allowed;

        // 2. check if feature is enabled
        var feats = new[] { BuiltInFeatures.PublicEditForm.Guid };
        var sysFeatures = sysFeatSvc.Value;
        if (userIsRestricted && !sysFeatures.IsEnabled(feats))
        {
            var error = $"low-permission users may not access this - {sysFeatures.MsgMissingSome(feats)}";
            preparedException = new PermissionDeniedException(error);
            return l.ReturnFalse();
        }
        preparedException = null;
        return l.ReturnTrue("ok");
    }

}