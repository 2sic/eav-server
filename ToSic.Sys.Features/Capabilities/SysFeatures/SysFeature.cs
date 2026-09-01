using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Utils;

namespace ToSic.Sys.Capabilities.SysFeatures;

/// <summary>
/// System feature definition.
/// These are features which the environment defines such as c# support; not configurable.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public record SysFeature : Feature
{
    /// <summary>
    /// Important prefix for all system features/capabilities,
    /// as they will be accessible in the features catalog and should be differentiated.
    /// </summary>
    public const string Prefix = "System";

    /// <summary>
    /// The unique name identifier for the system feature.
    /// </summary>
    public override required string NameId
    {
        get => base.NameId;
        init => base.NameId = EnsureNameIdPrefix(value);
    }
    
    /// <summary>
    /// Ensure that all these system features have a prefix; add if missing.
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    private static string EnsureNameIdPrefix(string original) 
        => original.IsEmptyOrWs() ? Prefix + "-Error-No-Name" : original.StartsWith(Prefix) ? original : $"{Prefix}-{original}";
    
    /// <summary>
    /// A link with further explanations about this feature, mainly used in error messages.
    /// </summary>
    public override string Link
    {
        get => field.UseFallbackIfNoValue(SysConstants.GoUrlSysFeats);
        init;
    }

    /// <summary>
    /// Indicates whether the feature is configurable.
    /// </summary>
    [PrivateApi("should not be visible, as it should just ensure that system features are marked as not configurable.")]
    public override bool IsConfigurable => false;

    [PrivateApi]
    public override string ToString() => $"{Prefix}: {Name} ({NameId} / {Guid})";
}