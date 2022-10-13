using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.WebApi.Cms
{
    public class UiData : IUiData
    {
        public UiData(IFeaturesInternal features) => _features = features;
        private readonly IFeaturesInternal _features;

        /// <summary>
        /// if the user has full edit permissions, he may also get the un-public features
        /// otherwise just the public Ui features
        /// </summary>
        /// <remarks>
        /// It's virtual, so other code can inherit from this. 
        /// </remarks>
        public virtual IList<FeatureState> Features(IContextOfApp appContext, IMultiPermissionCheck permCheck)
        {
            var userHasPublishRights = permCheck.UserMayOnAll(GrantSets.WritePublished);
            return appContext.User.IsSiteAdmin
                ? _features.All.ToList()
                : _features.EnabledUi.Where(f => userHasPublishRights || f.Public).ToList();
        }
    }
}
