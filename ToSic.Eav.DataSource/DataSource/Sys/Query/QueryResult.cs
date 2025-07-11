﻿namespace ToSic.Eav.DataSource.Sys.Query;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryResult(IDataSource main, Dictionary<string, IDataSource> dataSources)
{
    public IDataSource Main => main;
    public  Dictionary<string, IDataSource> DataSources => dataSources;
}