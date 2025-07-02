using System.Text.Json.Serialization;

namespace ToSic.Eav.Apps.Assets.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class File<TFolderId, TFileId>: IFile<TFolderId, TFileId>
{
    /// <inheritdoc />
    [JsonIgnore]
    public required TFileId SysId { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public required TFolderId ParentSysId {get; init; }

    public required string Folder { get; init; }

    #region Old Interface, where IDs were always ints

    public int Id => SysId as int? ?? 0;
    public int ParentId => ParentSysId as int? ?? 0;
    public int FolderId => ParentSysId as int? ?? 0;

    #endregion

    public required string FullName { get; init; }
    public required string Extension { get; init; }

    public required string Path { get; init; }

    public required DateTime Created { get; init; }


    public required DateTime Modified { get; init; }

    public required string Name { get; init; }

    public int Size { get; init; }

    public ISizeInfo SizeInfo => _sizeInfo.Get(() => new SizeInfo(Size))!;
    private readonly GetOnce<ISizeInfo> _sizeInfo = new();

    [JsonIgnore] // This should never get streamed to a json if people just return the object in a WebApi
    public required string PhysicalPath { get; init; }

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