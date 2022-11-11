using System;
using System.Text;

namespace ToSic.Lib.Logging
{
    public static partial class LogExtensions
    {
        /// <summary>
        /// Dump result to an internal format - not very important in public use cases
        /// </summary>
        public static string Dump(
            this ILog log,
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
            log.Entries.ForEach(e => lg.AppendLine(e.Source
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
