namespace ToSic.Eav.DataSource.Internal.Query;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class QueryResult(IDataSource main, Dictionary<string, IDataSource> dataSources)
{
    public IDataSource Main => main;
    public  Dictionary<string, IDataSource> DataSources => dataSources;
}