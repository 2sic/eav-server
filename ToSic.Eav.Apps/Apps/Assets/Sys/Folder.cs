using System.Text.Json.Serialization;

namespace ToSic.Eav.Apps.Assets.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class Folder<TFolderId, TFileId> : IFolder<TFolderId, TFileId>
{
    /// <inheritdoc/>
    int IAsset.Id => SysId as int? ?? 0;

    /// <inheritdoc/>
    int IAsset.ParentId => ParentSysId as int? ?? 0;

    [JsonIgnore]
    public required TFolderId ParentSysId { get; init; }

    [JsonIgnore]
    public required TFolderId SysId { get; init; }


    /// <inheritdoc/>
    public virtual bool HasChildren { get; init; }

    /// <inheritdoc/>
    public required DateTime Created { get; init; }

    /// <inheritdoc/>
    public required DateTime Modified { get; init; }

    /// <inheritdoc/>
    public required string Path { get; init; }
        
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    [JsonIgnore] // This should never get streamed to a json if people just return the object in a WebApi
    public required string PhysicalPath { get; init; }

    #region not available properties which existed on the previous DNN interface

    // These commented out fields are what DNN would have and they have problems
    // 1. they are basically meaningless for output
    // 2. they probably don't exist in a similer model a non-dnn system
    // 3. They often have inconsistent naming, like "ID" instead of "Id"

    //public string DisplayPath { get; internal init; }
    //public string DisplayName { get; internal init; }
    //public int StorageLocation { get; internal init; }
    //public int FolderMappingID { get; internal init; }
    //public int PortalID { get; internal init; }
    //public int ParentID { get; internal init; }
    //public int WorkflowID { get; internal init; }
    //public string MappedPath { get; internal init; }
    //public bool IsVersioned { get; internal init; }
    //public bool IsCached { get; internal init; }

    //public Guid VersionGuid { get; internal init; }
    //public Guid UniqueId { get; internal init; }
    //public bool IsProtected { get; internal init; }
    //public int KeyID { get; internal init; }

    #endregion
}