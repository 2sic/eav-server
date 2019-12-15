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
        public int Count;
        public bool Error = false;

        public StreamInfo(IDataStream strm, IDataTarget target, string inName)
        {
            try
            {
                Target = (target as IDataSource).Guid;
                Source = strm.Source.Guid;
                TargetIn = inName;
                foreach (var outStm in strm.Source.Out)
                    if (outStm.Value == strm)
                        SourceOut = outStm.Key;

                Count = strm.List.Count();
            }
            catch
            {
                Error = true;
            }
        }
    }
}
