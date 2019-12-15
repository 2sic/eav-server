namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Just a wrapper-class, so systems can differentiate between live and deferred streams
    /// </summary>
    public class DataStreamDeferred: DataStream
    {
        public DataStreamDeferred(IDataSource source, string name, GetIEnumerableDelegate listDelegate = null, bool enableAutoCaching = false) : base(source, name, listDelegate, enableAutoCaching)
        {
        }
    }
}
