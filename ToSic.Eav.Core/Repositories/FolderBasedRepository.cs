using System.Collections.Generic;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Repositories
{
    public abstract class FolderBasedRepository: RepositoryBase
    {
        /// <summary>
        /// Empty constructor is very important, as this is typically used by inheriting classes
        /// </summary>
        protected FolderBasedRepository() : this(true, true, null) { }

        protected FolderBasedRepository(bool global, bool readOnly, ILog parentLog) 
            : base(global, readOnly, RepositoryTypes.Folder, parentLog) { }

        public abstract List<string> RootPaths { get; }
    }
}
