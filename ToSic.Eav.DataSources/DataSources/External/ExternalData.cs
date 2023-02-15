using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Base DataSource class for providing data from external sources.
    /// </summary>
    /// <remarks>
    /// This has changed a lot in v15 (breaking change).
    /// Read about it in the docs.
    /// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract class ExternalData: DataSource
    {

        /// <summary>
        /// Initializes an DataSource which will usually provide/generate external data.
        /// </summary>
        /// <param name="dependencies">Dependencies needed by this data source and/or the parent</param>
        /// <param name="logName">
        /// The log name/identifier for insights logging.
        /// Optional, but makes debugging a bit easier when provided.
        /// </param>
        /// <remarks>
        /// set the cache creation date to the moment the object is constructed
        /// this is important, because the date should stay fixed throughout the lifetime of this object
        /// but renew when it is updates
        /// </remarks>
        [PrivateApi]
        protected ExternalData(Dependencies dependencies, string logName = null) : base(dependencies, logName ?? $"{DataSourceConstants.LogPrefix}.Extern")
        {
            CacheTimestamp = DateTime.Now.Ticks;
        }

        /// <inheritdoc />
        public override long CacheTimestamp { get; }
    }
}
