using System;
using System.Linq;

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

        protected readonly IDataStream Stream;

        public StreamInfo(IDataStream stream, IDataTarget target, string inName)
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
