using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Just a wrapper-class, so systems can differentiate between live and deferred streams
    /// </summary>
    public class DataStreamDeferred: DataStream
    {
        /// <summary>
        /// The Cache-Suffix helps to keep these streams separate in case the original stream also says it caches
        /// Because then they would have the same cache-key, and that would cause trouble
        /// </summary>
        [PrivateApi] public override string CacheSuffix => "!Deferred";

        public DataStreamDeferred(IDataSource source, string name, GetIEnumerableDelegate listDelegate = null, bool enableAutoCaching = false) : base(source, name, listDelegate, enableAutoCaching)
        {
        }
    }
}
