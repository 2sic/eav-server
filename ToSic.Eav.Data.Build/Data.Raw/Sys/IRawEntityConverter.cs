namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// WIP v22 2dm
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRawEntityConverter
{
    IRawEntity Convert<TSource>(TSource source, RawConvertOptions options)
        where TSource : class;
}