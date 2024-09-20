using ToSic.Eav.Context;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Context;

namespace ToSic.Eav.WebApi.Cms;

internal class UiData(IEavFeaturesService features, IUser user) : IUiData
{
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
        => (user.IsSiteAdmin
                ? features.All
                : userHasPublishRights
                    ? features.UiFeaturesForEditors
                    : features.UiFeaturesForEditors.Where(f => f.IsPublic))
            .OrderBy(f => f.NameId)
            .ToList();

    public IList<FeatureDto> FeaturesDto(bool userHasPublishRights, bool forSystemTypes)
        => Features(userHasPublishRights)
            .Select(f => new FeatureDto(f, forSystemTypes))
            .ToList();
}