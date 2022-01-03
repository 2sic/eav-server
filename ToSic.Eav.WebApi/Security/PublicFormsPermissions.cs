﻿using System;
using ToSic.Eav.Configuration;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Errors;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Sxc.WebApi.Security
{
    internal static class PublicFormsPermissions
    {
        internal static bool UserCanWriteAndPublicFormsEnabled(this MultiPermissionsApp mpa, IServiceProvider sp, out HttpExceptionAbstraction preparedException, out string error)
        {
            var wrapLog = mpa.Log.Call("");
            // 1. check if user is restricted
            var userIsRestricted = !mpa.UserMayOnAll(GrantSets.WritePublished);

            // 2. check if feature is enabled
            var feats = new[] { FeaturesCatalog.PublicEditForm.Id };
            var sysFeatures = sp.Build<IFeaturesInternal>();
            if (userIsRestricted && !sysFeatures.Enabled(feats))
            {
                error = $"low-permission users may not access this - {sysFeatures.MsgMissingSome(feats)}";
                preparedException = HttpException.PermissionDenied(error);
                return false;
            }
            wrapLog("ok");
            preparedException = null;
            error = null;
            return true;
        }

    }
}
