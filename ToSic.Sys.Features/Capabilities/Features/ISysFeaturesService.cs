using System.Diagnostics.CodeAnalysis;
using ToSic.Sys.Caching;

namespace ToSic.Sys.Capabilities.Features;

[PrivateApi("Internal stuff only")]
public interface ISysFeaturesService: ICacheExpiring, ICanBeCacheDependency
{
    #region WIP trying to remove the old IFeaturesService from our used API - this is temporary - should rename later as is better

    /// <summary>
    /// Checks if a feature is enabled
    /// </summary>
    /// <param name="guid">The feature Guid</param>
    /// <returns>true if the feature is enabled</returns>
    bool IsEnabled(Guid guid);

    /// <summary>
    /// Checks if a list of features are enabled, in case you need many features to be activated.
    /// </summary>
    /// <param name="guids">list/array of Guids</param>
    /// <returns>true if all features are enabled, false if any one of them is not</returns>
    bool IsEnabled(IEnumerable<Guid> guids);


    /// <summary>
    /// Informs you if the enabled features are valid or not - meaning if they have been countersigned by the 2sxc features system.
    /// As of now, it's not enforced, but in future it will be. 
    /// </summary>
    /// <returns>true if the features were signed correctly</returns>
    bool Valid { get; }

    #endregion


    [PrivateApi]
    IEnumerable<FeatureState> All { get; }

    [PrivateApi]
    IEnumerable<FeatureState> UiFeaturesForEditors { get; }


    [PrivateApi]
    bool IsEnabled(IEnumerable<Guid> features, string message, [NotNullWhen(true)] out FeaturesDisabledException? exception);

    [PrivateApi]
    string MsgMissingSome(params Guid[] ids);

    /// <summary>
    /// Checks if a list of features are enabled, in case you need many features to be activated.
    /// </summary>
    /// <param name="nameIds">list/array of name IDs</param>
    /// <returns>true if all features are enabled, false if any one of them is not</returns>
    /// <remarks>
    /// Added in v13.01
    /// </remarks>
    [PrivateApi("Hide - was never public on this interface")]
    bool IsEnabled(params string[] nameIds);

    /// <summary>
    /// Get a feature state or return null if not found.
    /// </summary>
    FeatureState? Get(string nameId);

    [PrivateApi("New in 13.05, not public at all")]
    bool IsEnabled(params Feature[] features);


    FeatureStatesPersisted? Stored { get; }

    bool UpdateFeatureList(FeatureStatesPersisted newList, List<FeatureState> sysFeatures);
}