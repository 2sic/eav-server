﻿using ToSic.Eav.Apps;


namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// This interface provides a standard for accessing hidden metadata items
/// We need it, because MetadataOf usually hides permissions in normal access
/// but when importing data, the items should be accessed to store in the DB
/// </summary>
[PrivateApi("not sure yet how to publish this api, if at all")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IMetadataInternals
{
    /// <summary>
    /// The complete list of metadata items, incl. the hidden ones
    /// </summary>
    ICollection<IEntity> AllWithHidden { get; }

    /// <summary>
    /// Context of metadata to be created.
    /// NOTE: type parameter is still not used, WIP
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IAppIdentity? Context(string type);

    [ShowApiWhenReleased(ShowApiMode.Never)]
    (int TargetType, ICollection<IEntity>? list, IHasMetadataSourceAndExpiring? appSource, Func<IHasMetadataSourceAndExpiring>? deferredSource) GetCloneSpecs();
}