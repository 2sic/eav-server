namespace ToSic.Eav.DataSources
{
    public interface IConnectable
    {
        IDataSourceConnection Connection { get; }
    }
}
