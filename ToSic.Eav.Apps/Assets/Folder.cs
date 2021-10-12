using System;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace ToSic.Eav.Apps.Assets
{
    public abstract class Folder<TFolderId, TFileId> : IFolder<TFolderId, TFileId>
    {
        /// <inheritdoc/>
        int IAsset.Id => SysId as int? ?? 0;

        /// <inheritdoc/>
        int IAsset.ParentId => ParentSysId as int? ?? 0;

        public TFolderId ParentSysId { get; set; }

        public TFolderId SysId { get; set; }


        /// <inheritdoc/>
        public virtual bool HasChildren { get; set; }

        /// <inheritdoc/>
        public DateTime Created { get; set; }

        /// <inheritdoc/>
        public DateTime Modified { get; set; }

        /// <inheritdoc/>
        public string Path { get; set; }
        
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        // [JsonIgnore] // This should never get streamed to a json if people just return the object in a WebApi
        public string PhysicalPath { get; set; }

        #region not available properties which existed on the previous DNN interface

        // These commented out fields are what DNN would have and they have problems
        // 1. they are basically meaningless for output
        // 2. they probably don't exist in a similer model a non-dnn system
        // 3. They often have inconsistent naming, like "ID" instead of "Id"

        //public string DisplayPath { get; internal set; }
        //public string DisplayName { get; internal set; }
        //public int StorageLocation { get; internal set; }
        //public int FolderMappingID { get; internal set; }
        //public int PortalID { get; internal set; }
        //public int ParentID { get; internal set; }
        //public int WorkflowID { get; internal set; }
        //public string MappedPath { get; internal set; }
        //public bool IsVersioned { get; internal set; }
        //public bool IsCached { get; internal set; }

        //public Guid VersionGuid { get; internal set; }
        //public Guid UniqueId { get; internal set; }
        //public bool IsProtected { get; internal set; }
        //public int KeyID { get; internal set; }

        #endregion
    }
}
