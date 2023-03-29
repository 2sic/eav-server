using System;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource
{
    [PrivateApi]
    public class DataSourceConnection
    {
        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public IDataSource DataSource;
        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public IDataSource DataTarget;
        
        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public string SourceStream { get; }
        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public string TargetStream { get; }
        
        /// <summary>
        /// Temporary safety net - unsure if usefull
        /// </summary>
        [JsonIgnore]
        public IDataStream DirectlyAttachedStream { get; }

        #region Serialization properties just for debugging in QueryInfo

        public QuickSourceInfo Source => new QuickSourceInfo(DataSource, SourceStream);
        public QuickSourceInfo Target => new QuickSourceInfo(DataTarget, TargetStream);

        #endregion

        public DataSourceConnection(IDataStream sourceStream, IDataSource target, string targetStream)
        {
            DirectlyAttachedStream = sourceStream;
            DataSource = sourceStream.Source;
            SourceStream = sourceStream.Name;
            DataTarget = target;
            TargetStream = targetStream;
        }


        public DataSourceConnection(IDataSource source, string sourceStream, IDataSource target, string targetStream)
        {
            DataSource = source;
            DataTarget = target;
            SourceStream = sourceStream;
            TargetStream = targetStream;
        }
    }
    
    
    [PrivateApi]
    public class QuickSourceInfo 
    {
        public QuickSourceInfo(IDataSource data, string streamName)
        {
            Label = data?.Label;
            Guid = data?.Guid;
            Name = data?.Name;
            Stream = streamName;
        }
        
        public string Label { get; }
        public Guid? Guid { get; }
        public string Name { get; }
        public string Stream { get; }
    }
}
