namespace ToSic.Sys.Capabilities.Features;

[PrivateApi("Internal stuff only")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILibFeaturesService
{
    bool IsEnabled(string nameIds);

    /// <summary>
    /// Get a feature state or return null if not found.
    /// </summary>
    FeatureState? Get(string nameId);

}