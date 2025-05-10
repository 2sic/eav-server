namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Marks objects which are not <see cref="IRawEntity"/>
/// but can provide one for automatic conversion.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Added in 15.04
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP for DataSources")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasRawEntity<out T>: IHasRawEntity where T: IRawEntity
{
    T RawEntity { get; }
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasRawEntity;