using System.Text.Json.Serialization;

namespace ToSic.Eav.DataSource.Sys;

[PrivateApi("Must be public, as it will be serialized in some DTOs")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataSourceConnection(IDataSource dataSource, string sourceStream, IDataSource dataTarget, string targetStream)
{
    public DataSourceConnection(IDataStream sourceStream, IDataSource target, string targetStream)
        : this(sourceStream.Source, sourceStream.Name, target, targetStream)
    {
        DirectlyAttachedStream = sourceStream;
    }

    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public IDataSource DataSource = dataSource;
    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public IDataSource DataTarget = dataTarget;
        
    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public string SourceStream { get; } = sourceStream;

    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public string TargetStream { get; } = targetStream;

    /// <summary>
    /// Temporary safety net - unsure if useful
    /// </summary>
    [JsonIgnore]
    public IDataStream? DirectlyAttachedStream { get; }

    #region Serialization properties just for debugging in QueryInfo

    public QuickSourceInfo Source => new(DataSource, SourceStream);
    public QuickSourceInfo Target => new(DataTarget, TargetStream);

    #endregion

}
    
    
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QuickSourceInfo(IDataSource data, string streamName)
{
    public string? Label { get; } = data?.Label;
    public Guid? Guid { get; } = data?.Guid;
    public string? Name { get; } = data?.Name;
    public string? Stream { get; } = streamName;
}