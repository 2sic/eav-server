using System.Collections.Generic;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Repositories
{
    public abstract class RepositoryInfoOfFolder: RepositoryInfo
    {
        /// <summary>
        /// Empty constructor is very important, as this is typically used by inheriting classes
        /// </summary>
        protected RepositoryInfoOfFolder() : this(true, true, null) { }

        protected RepositoryInfoOfFolder(bool global, bool readOnly, Log parentLog) 
            : base(global, readOnly, RepositoryTypes.Folder, parentLog) { }

        public abstract List<string> RootPaths { get; }
    }
}
