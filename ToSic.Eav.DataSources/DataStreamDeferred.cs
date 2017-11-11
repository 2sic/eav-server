namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Just a wrapper-class, so systems can differentiate between live and deferred streams
    /// </summary>
    public class DataStreamDeferred: DataStream
    {
        public DataStreamDeferred(IDataSource source, string name, /*GetDictionaryDelegate dictionaryDelegate,*/ GetIEnumerableDelegate lightListDelegate = null, bool enableAutoCaching = false) : base(source, name, lightListDelegate, enableAutoCaching)
        {
        }

        //public DataStreamDeferred(IDataSource owner, string name, DataSource upStream)
        //{
            
        //}
    }
}
