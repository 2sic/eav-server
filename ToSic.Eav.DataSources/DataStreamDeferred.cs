namespace ToSic.Eav.DataSources
{
    public class DataStreamDeferred: DataStream
    {
        public DataStreamDeferred(IDataSource source, string name, GetDictionaryDelegate dictionaryDelegate, GetIEnumerableDelegate lightListDelegate = null, bool enableAutoCaching = false) : base(source, name, dictionaryDelegate, lightListDelegate, enableAutoCaching)
        {
        }

        //public DataStreamDeferred(IDataSource owner, string name, DataSource upStream)
        //{
            
        //}
    }
}
