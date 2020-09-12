namespace ToSic.Eav.Apps.Assets
{
    /// <summary>
    /// This is to enable generic id interfaces
    /// It enhances the existing int-based IDs for other use cases which don't use ints
    /// </summary>
    /// <typeparam name="TParentSysId"></typeparam>
    public interface IAssetWithParentSysId<out TParentSysId>: IAsset
    {
        /// <summary>
        /// The folder ID of the file, or parent-folder of a folder, if the underlying environment uses int IDs
        /// </summary>
        /// <returns>an int with the id used by the environment to track this item</returns>
        // ReSharper disable once UnusedMemberInSuper.Global
        TParentSysId ParentSysId { get; }
    }
}
