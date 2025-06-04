using ToSic.Eav.WebApi.Context;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.WebApi.Cms;

/// <summary>
/// Helper service to retrieve all the UI features for the edit UI
/// </summary>
public interface IUiData
{
    /// <summary>
    /// if the user has full edit permissions, he may also get the un-public features
    /// otherwise just the public Ui features
    /// </summary>
    IList<FeatureState> Features(IMultiPermissionCheck permCheck);

    IList<FeatureDto> FeaturesDto(bool userHasPublishRights, bool forSystemTypes);
}