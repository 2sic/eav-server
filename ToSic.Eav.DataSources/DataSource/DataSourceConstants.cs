namespace ToSic.Eav.DataSource;

/// <summary>
/// Various constants typically used in/for DataSources.
/// </summary>
/// <remarks>
/// History
/// * Created ca. v10
/// * Accidentally marked private in 16.09
/// * Re-published in 19.01
/// </remarks>
[PublicApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataSourceConstants
{

    #region Constants for in the DataSource

    /// <summary>
    /// Correct prefix to use when retrieving a value from the current data sources configuration entity.
    /// Always use this variable, don't ever write the name as a string, as it could change in the future.
    /// </summary>
    public const string MyConfigurationSourceName = "MyConfiguration";

    #endregion

    /// <summary>
    /// Default In-/Out-Stream Name
    /// </summary>
    public const string StreamDefaultName = "Default";

    /// <summary>
    /// Very common stream name for fallback streams.
    /// </summary>
    public const string StreamFallbackName = "Fallback";


    #region Query / Visual Query

    /// <summary>
    /// Use this in the `In` stream names array of the <see cref="VisualQueryAttribute"/>
    /// to mark an in-stream as being required.
    /// </summary>
    [PublicApi]
    public const string InStreamRequiredSuffix = "*";

    /// <summary>
    /// Marker for specifying that the Default `In` stream is required on the <see cref="VisualQueryAttribute"/>.
    /// </summary>
    [PublicApi]
    public const string InStreamDefaultRequired = "Default" + InStreamRequiredSuffix;

    /// <summary>
    /// The source name to get query parameters.
    /// Usually used in tokens like `[Params:MyParamKey]`
    /// </summary>
    [PublicApi]
    public const string ParamsSourceName = "Params";

    #endregion
}