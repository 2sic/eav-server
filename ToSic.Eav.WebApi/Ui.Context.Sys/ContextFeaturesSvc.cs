using ToSic.Eav.Ui.Context.Sys.Dto;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Users;

namespace ToSic.Eav.Ui.Context.Sys;

public class ContextFeaturesSvc(ISysFeaturesService features, IUser user)
{
    /// <summary>
    /// Get the features DTO
    /// </summary>
    /// <param name="userHasPublishRights"></param>
    /// <param name="forSystemTypes"></param>
    /// <returns></returns>
    public IList<FeatureDto> FeaturesDto(bool userHasPublishRights, bool forSystemTypes)
        => Features(userHasPublishRights)
            .Select(f => new FeatureDto(f, forSystemTypes))
            .ToList();

    /// <summary>
    /// Helper
    /// </summary>
    /// <param name="userHasPublishRights"></param>
    /// <returns></returns>
    private IList<FeatureState> Features(bool userHasPublishRights)
        => (user.IsSiteAdmin
                ? features.All
                : userHasPublishRights
                    ? features.UiFeaturesForEditors
                    : features.UiFeaturesForEditors.Where(f => f.IsPublic))
            .OrderBy(f => f.NameId)
            .ToList();

}