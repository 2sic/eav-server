using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Base DataSource class for providing data from external systems
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract class ExternalData: DataSourceBase
    {
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => "DS.Extern";

        /// <summary>
        /// Initializes an external data source.
        /// </summary>
        /// <remarks>
        /// set the creation date to the moment the object is constructed
        /// this is important, because the date should stay fixed throughout the lifetime of this object
        /// but renew when it is updates
        /// </remarks>
        [PrivateApi]
        protected ExternalData() => CacheTimestamp = DateTime.Now.Ticks;

        /// <inheritdoc />
        public override long CacheTimestamp { get; }
    }
}
