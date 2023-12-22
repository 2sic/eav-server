using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Context;

namespace ToSic.Eav.WebApi.Cms;

internal class UiData : IUiData
{
    public UiData(IEavFeaturesService features, IUser user)
    {
        _features = features;
        _user = user;
    }

    private readonly IEavFeaturesService _features;
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
                    : _features.UiFeaturesForEditors.Where(f => f.IsPublic))
            .OrderBy(f => f.NameId)
            .ToList();

    public IList<FeatureDto> FeaturesDto(bool userHasPublishRights)
        => Features(userHasPublishRights).Select(f => new FeatureDto(f)).ToList();
}