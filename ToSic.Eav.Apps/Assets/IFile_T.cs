namespace ToSic.Eav.Apps.Assets
{
    public interface IFile<out TFolderId, out TFileId> : IFile, IAssetSysId<TFolderId, TFileId>
    {
    }
}
