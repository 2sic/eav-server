namespace ToSic.Eav.DataSources
{
    public partial class DataSource: IDataSourceConnection
    {
        IDataSource IDataSourceConnection.DataSource => this;

        string IDataSourceConnection.SourceStreamName => DataSourceConstants.AllStreams;
        string IDataSourceConnection.TargetStreamName => DataSourceConstants.AllStreams;

        IDataStream IDataSourceConnection.Stream => this.GetStream();

        bool IDataSourceConnection.HasMore => false;
    }
}
