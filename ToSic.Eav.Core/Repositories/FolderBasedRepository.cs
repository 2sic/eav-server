using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Repositories
{
    /// <summary>
    /// Use this to tell the EAV at boot time that there is another folder based repository.
    /// This will cause the EAV to load that folders Content-Types and Queries.
    /// </summary>
    [PublicApi]
    public abstract class FolderBasedRepository: RepositoryBase
    {
        /// <summary>
        /// Empty constructor is very important, as this is typically used by inheriting classes
        /// </summary>
        protected FolderBasedRepository() : this(true, true, null) { }

        /// <summary>
        /// This constructor could provide more data, but as of now there is only one possible configuration.
        /// </summary>
        /// <param name="global">Must always be true</param>
        /// <param name="readOnly">Must always be true</param>
        /// <param name="parentLog">A parent logger to log what happens - usually null for such a global startup object</param>
        [PrivateApi]
        protected FolderBasedRepository(bool global, bool readOnly, ILog parentLog) 
            : base(global, readOnly, RepositoryTypes.Folder, parentLog) { }

        public abstract List<string> RootPaths { get; }
    }
}


namespace ToSic.Eav.Repositories
{
    [Obsolete("please use FolderBasedRepository in future")]
    [PrivateApi]
    public abstract class RepositoryInfoOfFolder : FolderBasedRepository
    {
        protected RepositoryInfoOfFolder(bool global, bool readOnly, ILog parentLog) 
        : base(global, readOnly, parentLog) { }
    }
}