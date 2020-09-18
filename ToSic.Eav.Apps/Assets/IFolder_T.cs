namespace ToSic.Eav.Apps.Assets
{
    public interface IFolder<out TFolderId, out TFileId>: IFolder, IAssetSysId<TFolderId>, IAssetWithParentSysId<TFolderId>
    {
    }
}
