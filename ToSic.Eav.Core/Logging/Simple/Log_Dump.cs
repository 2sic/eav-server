using System;
using System.Text;

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
                                               + (e.Elapsed != TimeSpan.Zero ? $" ⌚ {e.Elapsed.TotalSeconds}s " : "" )
                                               + (withCaller && e.Code != null ? $"{callStart}{e.Code.Path} - {e.Code.Name}() #{e.Code.Line}{callEnd}" : "")
            ));
            lg.Append(end);
            return lg.ToString();
        }
    }
}
