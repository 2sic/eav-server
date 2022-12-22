using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Context;

namespace ToSic.Eav.WebApi.Cms
{
    public class UiData : IUiData
    {
        public UiData(IFeaturesInternal features, IUser user)
        {
            _features = features;
            _user = user;
        }

        private readonly IFeaturesInternal _features;
        private readonly IUser _user;

        /// <summary>
        /// if the user has full edit permissions, he may also get the un-public features
        /// otherwise just the public Ui features
        /// </summary>
        /// <remarks>
        /// It's virtual, so other code can inherit from this. 
        /// </remarks>
        public IList<FeatureState> Features(IMultiPermissionCheck permCheck)
        {
            var userHasPublishRights = permCheck.UserMayOnAll(GrantSets.WritePublished);
            return Features(userHasPublishRights);
        }

        private IList<FeatureState> Features(bool userHasPublishRights)
            => (_user.IsSiteAdmin
                    ? _features.All
                    : userHasPublishRights
                        ? _features.UiFeaturesForEditors
                        : _features.UiFeaturesForEditors.Where(f => f.Public))
                .OrderBy(f => f.NameId)
                .ToList();

        public IList<FeatureDto> FeaturesDto(bool userHasPublishRights)
            => Features(userHasPublishRights)
                .Select(f => new FeatureDto
                {
                    NameId = f.NameId,
                    Enabled = f.Enabled,
                    Name = f.Name,
                })
                .ToList();
    }
}
