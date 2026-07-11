namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// Should mark any object/interface which can be converted to a raw entity.
/// </summary>
/// <remarks>
/// This is a marker interface, and must implement one of the other interfaces to work properly.
/// These include:
/// * <see cref="IGetRawConverter"/> - for objects which can provide a converter for themselves
/// * <see cref="IRawEntity"/> - for objects which are already a raw entity
/// </remarks>
[PrivateApi]
public interface IConvertibleToRawEntity;

public static class ConvertibleToRawEntityExtensions
{
    // Extension methods for IConvertibleToRawEntity can be added here
    public static IRawEntity GetRawEntity<TSource>(this TSource source, RawConvertOptions options)
        where TSource : class, IConvertibleToRawEntity =>
        source switch
        {
            IGetRawConverter getConverter => getConverter
                .GetConverter()
                .TryRawEntity(source, options),
            IRawEntity rawEntity => rawEntity,
            _ => throw new InvalidOperationException($"Cannot convert to raw entity. The source must implement {nameof(IRawEntity)} or {nameof(IGetRawConverter)}")
        };
}