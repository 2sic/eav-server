using System.Text;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        public string Dump(string separator = " - ", string start = "", string end = "", string resultStart = "=>", string resultEnd = "")
        {
            var lg = new StringBuilder(start);
            Entries.ForEach(e => lg.AppendLine(e.Source
                                               + separator
                                               + new string('~', e.Depth * 2)
                                               + e.Message
                                               + (e.Result != null ? resultStart + e.Result + resultEnd: string.Empty)
            ));
            lg.Append(end);
            return lg.ToString();
        }
    }
}
