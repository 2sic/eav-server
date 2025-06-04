using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Marks objects which are not <see cref="IRawEntity"/>
/// but can provide one for automatic conversion.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Added in 15.04
/// Note: because of changes in v19 and v20, this doesn't seem to be in use ATM.
/// There is still code which would convert objects having this, but ATM it seems to be unused.
/// It's unclear if we will pick this up again, for now better keep it internal as we may remove it again. 
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP for DataSources")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasRawEntity<out T>: IHasRawEntity where T: IRawEntity
{
    T RawEntity { get; }
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasRawEntity;