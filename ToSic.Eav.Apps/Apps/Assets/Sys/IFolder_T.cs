namespace ToSic.Eav.Apps.Assets.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IFolder<out TFolderId, out TFileId>: IFolder, IAssetSysId<TFolderId>, IAssetWithParentSysId<TFolderId>;