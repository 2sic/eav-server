namespace ToSic.Eav.DataSource.Internal;

public class DataSourceConstantsInternal
{
    /// <summary>
    /// Prefix to use for all built-in data sources.
    /// </summary>
    [PrivateApi] internal const string LogPrefix = "DS";

    #region Version Change Constants (internal)

    [PrivateApi] internal const string V3To4DataSourceDllOld = ", ToSic.Eav";
    [PrivateApi] internal const string V3To4DataSourceDllNew = ", ToSic.Eav.DataSources";

    /// <summary>
    /// Global queries must start with this prefix
    /// </summary>
    [PrivateApi] internal const string SystemQueryPrefixPreV15 = "Eav.Queries.Global.";

    [PrivateApi] internal const string SystemQueryPrefix = "System.";

    [PrivateApi] public static bool IsGlobalQuery(string name) => name.StartsWith(SystemQueryPrefixPreV15) || name.StartsWith(SystemQueryPrefix);

    #endregion

    #region Stream names 

    
    internal const string AllStreams = "***";
    
    /// <summary>
    /// PublishedEntities Stream Name
    /// </summary>
    internal const string StreamPublishedName = "Published";

    /// <summary>
    /// Draft-Entities Stream Name
    /// </summary>
    internal const string StreamDraftsName = "Drafts";

    #endregion
}