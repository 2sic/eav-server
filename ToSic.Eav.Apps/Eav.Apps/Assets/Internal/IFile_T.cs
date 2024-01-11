namespace ToSic.Eav.Apps.Assets.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IFile<out TFolderId, out TFileId> : IFile, IAssetSysId<TFileId>, IAssetWithParentSysId<TFolderId>
{
}