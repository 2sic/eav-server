namespace ToSic.Eav.Apps.Assets
{
    public interface IFile<TFolderId, TFileId> : IFile, IAssetSysId<TFolderId, TFileId>
    {
        /// <summary>
        /// The folder ID of the file, if the underlying environment uses int IDs
        /// </summary>
        /// <returns>an int with the id used by the environment to track this item</returns>
        // ReSharper disable once UnusedMemberInSuper.Global
        new TFolderId FolderId { get; set; }

    }
}
