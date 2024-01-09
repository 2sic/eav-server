using System;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataSourceConnection(IDataSource source, string sourceStream, IDataSource target, string targetStream)
{
    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public IDataSource DataSource = source;
    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public IDataSource DataTarget = target;
        
    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public string SourceStream { get; } = sourceStream;

    [JsonIgnore]    // don't try to serialize, as it's too large of an object
    public string TargetStream { get; } = targetStream;

    /// <summary>
    /// Temporary safety net - unsure if useful
    /// </summary>
    [JsonIgnore]
    public IDataStream DirectlyAttachedStream { get; }

    #region Serialization properties just for debugging in QueryInfo

    public QuickSourceInfo Source => new(DataSource, SourceStream);
    public QuickSourceInfo Target => new(DataTarget, TargetStream);

    #endregion

    public DataSourceConnection(IDataStream sourceStream, IDataSource target, string targetStream) : this(sourceStream.Source, sourceStream.Name, target, targetStream)
    {
        DirectlyAttachedStream = sourceStream;
    }
}
    
    
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class QuickSourceInfo(IDataSource data, string streamName)
{
    public string Label { get; } = data?.Label;
    public Guid? Guid { get; } = data?.Guid;
    public string Name { get; } = data?.Name;
    public string Stream { get; } = streamName;
}