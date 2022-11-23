using System;
using System.Text;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        public string Dump(
            string separator = " - ", 
            string start = "", 
            string end = "", 
            string resultStart = "=>", 
            string resultEnd = "",
            bool withCaller  = false,
            string callStart = "",
            string callEnd = ""
        )
        {
            var lg = new StringBuilder(start);
            Entries.ForEach(e => lg.AppendLine(e.Source
                                               + separator
                                               + new string('~', e.Depth * 2)
                                               + e.Message
                                               + (e.Result != null ? resultStart + e.Result + resultEnd: string.Empty)
                                               + EntryTime(e)
                                               + (withCaller && e.Code != null ? $"{callStart}{e.Code.Path} - {e.Code.Name}() #{e.Code.Line}{callEnd}" : "")
            ));
            lg.Append(end);
            return lg.ToString();
        }

        private static string EntryTime(Entry e) => e.Elapsed != TimeSpan.Zero ? $" ⌚ {e.Elapsed.TotalSeconds}s " : "";
    }
}
