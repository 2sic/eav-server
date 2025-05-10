namespace ToSic.Eav.Apps.Assets.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IFile<out TFolderId, out TFileId> : IFile, IAssetSysId<TFileId>, IAssetWithParentSysId<TFolderId>
{
}