using System.Collections.Generic;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Repositories
{
    public abstract class RepositoryInfoOfFolder: RepositoryInfo
    {
        protected RepositoryInfoOfFolder(bool global, bool readOnly, Log parentLog) 
            : base(global, readOnly, RepositoryTypes.Folder, parentLog) { }

        public abstract List<string> RootPaths { get; }
    }
}
