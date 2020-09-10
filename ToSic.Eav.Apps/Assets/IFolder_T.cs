namespace ToSic.Eav.Apps.Assets
{
    public interface IFolder<TFolderId, TFileId>: IFolder, IAssetSysId<TFolderId, TFolderId>
    {
    }
}
