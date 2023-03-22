using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Raw;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Various constants typically used in/for DataSources.
    /// </summary>
    [PublicApi]
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

        #endregion

        #region Stream names - all public

        /// <summary>
        /// Default In-/Out-Stream Name
        /// </summary>
        public const string StreamDefaultName = "Default";


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

        /// <summary>
        /// Constant empty list of entities - for common scenarios where we just need to return empty results.
        /// </summary>
        public static IImmutableList<IEntity> EmptyList = ImmutableList<IEntity>.Empty;

        public static IImmutableList<IRawEntity> EmptyRawList = ImmutableList<IRawEntity>.Empty;

        #endregion
    }
}
