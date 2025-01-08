namespace ToSic.Eav.DataSource.Internal;

/// <summary>
/// Various constants typically used in/for DataSources.
/// </summary>
[PrivateApi("was marked as public till 16.09")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataSourceConstants
{
    /// <summary>
    /// Prefix to use for all built-in data sources.
    /// </summary>
    [PrivateApi]
    internal const string LogPrefix = "DS";

    #region Version Change Constants (internal)

    [PrivateApi] internal const string V3To4DataSourceDllOld = ", ToSic.Eav";
    [PrivateApi] internal const string V3To4DataSourceDllNew = ", ToSic.Eav.DataSources";

    /// <summary>
    /// Global queries must start with this prefix
    /// </summary>
    [PrivateApi] internal const string SystemQueryPrefixPreV15 = "Eav.Queries.Global.";
    [PrivateApi] internal const string SystemQueryPrefix = "System.";

    [PrivateApi]
    public static bool IsGlobalQuery(string name) => name.StartsWith(SystemQueryPrefixPreV15) || name.StartsWith(SystemQueryPrefix);

    #endregion

    #region Constants for in the DataSource

    /// <summary>
    /// Correct prefix to use when retrieving a value from the current data sources configuration entity.
    /// Always use this variable, don't ever write the name as a string, as it could change in future.
    /// </summary>
    public const string MyConfigurationSourceName = "MyConfiguration";

    #endregion

    #region Stream names - all public

    /// <summary>
    /// Default In-/Out-Stream Name
    /// </summary>
    public const string StreamDefaultName = "Default";

    [PrivateApi] internal const string AllStreams = "***";

    /// <summary>
    /// Very common stream name for fallback streams.
    /// </summary>
    public const string StreamFallbackName = "Fallback";


    /// <summary>
    /// PublishedEntities Stream Name
    /// </summary>
    internal const string StreamPublishedName = "Published";

    /// <summary>
    /// Draft-Entities Stream Name
    /// </summary>
    internal const string StreamDraftsName = "Drafts";

    #endregion

    #region Empty Lists

    ///// <summary>
    ///// Constant empty list of entities - for common scenarios where we just need to return empty results.
    ///// </summary>
    //public static IImmutableList<IEntity> EmptyList = ImmutableList<IEntity>.Empty;
    //public static IImmutableList<IRawEntity> EmptyRawList = ImmutableList<IRawEntity>.Empty;

    #endregion

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
    [PublicApi] public const string ParamsSourceName = "Params";

    #endregion
}