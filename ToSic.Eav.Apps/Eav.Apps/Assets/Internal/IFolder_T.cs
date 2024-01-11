namespace ToSic.Eav.Apps.Assets.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IFolder<out TFolderId, out TFileId>: IFolder, IAssetSysId<TFolderId>, IAssetWithParentSysId<TFolderId>
{
}