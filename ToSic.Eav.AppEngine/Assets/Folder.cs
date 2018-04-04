using System;

namespace ToSic.Eav.Apps.Assets
{
    public class Folder
    {
        /// <summary>
        /// An Id - only relevant if the environment uses Ids
        /// </summary>
        public int Id { get; set; }

        public bool HasChildren { get; set; }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string FolderPath { get; set; }
        public string Name { get; set; }


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
