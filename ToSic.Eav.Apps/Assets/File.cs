﻿using System;

namespace ToSic.Eav.Apps.Assets
{
    public abstract class File : IFile
    {
        /// <summary>
        /// An Id - only relevant if the environment uses Ids
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Folder Id, only relevant if the environment uses Ids
        /// Internally we want to prefer parentId, but
        /// as this may confuse people and because FolderId may already be publicly used
        /// we'll keep this property indefinitely
        /// </summary>
        public int FolderId { get; set; }

        public int ParentId => FolderId;

        public string Folder { get; set; }

        public string FullName { get; set; }
        public string Extension { get; set; }

        public string Path { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public string Name { get; set; }

        public int Size { get; set; }


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
