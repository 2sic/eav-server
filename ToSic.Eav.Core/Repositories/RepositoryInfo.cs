using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Repositories
{
    public class RepositoryInfo: HasLog
    {
        public RepositoryInfo(bool global, bool readOnly, RepositoryTypes type, Log parentLog) : base("RP.Info", parentLog)
        {
            Global = global;
            ReadOnly = readOnly;
            Type = type;
        }
        

        public bool Global { get; }

        public RepositoryTypes Type { get; }

        public bool ReadOnly { get; }
    }
}
