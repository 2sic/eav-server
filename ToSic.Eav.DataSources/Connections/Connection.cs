using System;
using System.Text.Json.Serialization;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    [PrivateApi("Experimental new connections between data sources")]
    public class Connection
    {
        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public IDataSource DataSource;
        [JsonIgnore]    // don't try to serialize, as it's too large of an object
        public IDataTarget DataTarget;
        
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

        public Connection(IDataStream sourceStream, IDataTarget target, string targetStream)
        {
            DirectlyAttachedStream = sourceStream;
            DataSource = sourceStream.Source;
            SourceStream = sourceStream.Name;
            DataTarget = target;
            TargetStream = targetStream;
        }


        public Connection(IDataSource source, string sourceStream, IDataTarget target, string targetStream)
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
        public QuickSourceInfo(IDataPartShared data, string streamName)
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
