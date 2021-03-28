using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Conversion;

namespace ToSic.Eav.DataSources.Debug
{
    public class StreamInfo
    {
        public Guid Target;
        public Guid Source;
        public string SourceOut;
        public string TargetIn;
        public int Count => Stream.List.Count();
        
        
        public bool Error = false;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> ErrorData;

        protected readonly IDataStream Stream;

        public StreamInfo(IDataStream stream, IDataTarget target, string inName, IEntitiesTo<Dictionary<string, object>> errorConverter)
        {
            try
            {
                Stream = stream;
                Target = (target as IDataSource)?.Guid ?? Guid.Empty;
                Source = stream.Source.Guid;
                TargetIn = inName;
                foreach (var outStm in stream.Source.Out)
                    if (outStm.Value == stream)
                        SourceOut = outStm.Key;

                var firstItem = Stream.List?.FirstOrDefault();
                Error = firstItem?.Type?.Name == DataSourceErrorHandling.ErrorType;
                if (Error) ErrorData = errorConverter.Convert(firstItem);
            }
            catch
            {
                Error = true;
            }
        }


        public bool Equals(StreamInfo comparison) =>
            comparison.Target == Target
            && comparison.Source == Source
            && comparison.TargetIn == TargetIn
            && comparison.SourceOut == SourceOut;
    }
}
