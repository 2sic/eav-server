using System.Collections.Generic;
using ToSic.Eav.Security;
using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Context;

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

    IList<FeatureDto> FeaturesDto(bool userHasPublishRights);
}