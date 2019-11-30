namespace ToSic.Eav.DataSources
{
    internal interface IDeferredDataSource
    {
        IDataStream DeferredOut(string name);
    }
}
