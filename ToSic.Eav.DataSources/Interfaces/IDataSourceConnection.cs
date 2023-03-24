namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// WIP interface to create one or many sources which can be attached when creating a new sources
    /// </summary>
    public interface IDataSourceConnection
    {
        IDataSource DataSource { get; }
        string SourceStreamName { get; }
        string TargetStreamName { get; }
        IDataStream Stream { get; }

        bool HasMore { get; }
    }
}
