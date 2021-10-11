using System;

namespace ToSic.Eav.Apps.Assets
{
    public abstract class File<TFolderId, TFileId>: IFile<TFolderId, TFileId>
    {
        /// <inheritdoc />
        public TFileId SysId { get; set; }

        /// <inheritdoc />
        public TFolderId ParentSysId {get; set; }

        public string Folder { get; set; }

        #region Old Interface, where IDs were always ints

        public int Id => SysId as int? ?? 0;
        public int ParentId => ParentSysId as int? ?? 0;
        public int FolderId => ParentSysId as int? ?? 0;

        #endregion

        public string FullName { get; set; }
        public string Extension { get; set; }

        public string Path { get; set; }

        public DateTime Created { get; set; }


        public DateTime Modified { get; set; }

        public string Name { get; set; }

        public int Size { get; set; }

        public string PhysicalPath { get; set; }

        #region not available properties which existed on the previous DNN interface
        // These commented out fields are what DNN would have and they have problems
        // 1. they are basically meaningless for output
        // 2. they probably don't exist in a similer model a non-dnn system
        // 3. They often have inconsistent naming, like "ID" instead of "Id"

        //public int StorageLocation { get; internal set; }
        //public int Width { get; internal set; }
        //public string SHA1Hash { get; internal set; }
        //public int PortalId { get; internal set; }

        //public bool IsCached { get; internal set; }

        //public int Height { get; internal set; }
        //public Guid VersionGuid { get; internal set; }

        //public Guid UniqueId { get; internal set; }
        //public string ContentType { get; internal set; }
        #endregion
    }
}
