﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi.Features
{
    internal class FeaturesHelpers
    {
        internal static IEnumerable<FeatureState> FeatureListWithPermissionCheck(IFeaturesInternal features, MultiPermissionsApp permCheck)
        {
            // if the user has full edit permissions, he may also get the un-public features
            // otherwise just the public Ui features
            var includeNonPublic = permCheck.UserMayOnAll(GrantSets.WritePublished);

            return features.EnabledUi.Where(f => includeNonPublic || f.Public == true);
        }
    }
}
