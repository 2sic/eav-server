namespace ToSic.Eav.Apps.Assets.Internal;

/// <summary>
/// This is to enable generic id interfaces
/// It enhances the existing int-based IDs for other use cases which don't use ints
/// </summary>
/// <typeparam name="TItemId"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAssetSysId<out TItemId>: IAsset
{
    /// <summary>
    /// The ID of the item, if the underlying environment uses int IDs
    /// </summary>
    /// <returns>an int with the id used by the environment to track this item</returns>
    TItemId SysId { get; }
}