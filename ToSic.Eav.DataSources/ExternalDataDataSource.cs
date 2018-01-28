using System;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Data source base class for providing data from external systems
    /// </summary>
    public class ExternalDataDataSource: BaseDataSource
    {
        public override string LogId => "DS.Extern";

        /// <inheritdoc />
        /// <remarks>
        /// set the creation date to the moment the object is constructed
        /// this is important, because the date should stay fixed throughout the lifetime of this object
        /// but renew when it is updates
        /// </remarks>
        public ExternalDataDataSource() => CacheTimestamp = DateTime.Now.Ticks;

        public override long CacheTimestamp { get; }
    }
}
