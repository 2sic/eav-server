namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// Extensions for any <see cref="IRawEntitySource"/>
/// </summary>
[PrivateApi]
public static class IRawEntitySourceExtensions
{

    /// <summary>
    /// Extension method, to convert any <see cref="IRawEntitySource"/> to a <see cref="IRawEntity"/>
    /// </summary>
    public static IRawEntity GetRawFromConverterOrDirectCast<TSource>(this TSource source, RawConvertOptions options)
        where TSource : class, IRawEntitySource =>
        source switch
        {
            IRawEntityConvertible getConverter => getConverter
                .GetConverter()
                .Convert(source, options),
            IRawEntity rawEntity => rawEntity,
            _ => throw new InvalidOperationException(
                $"Cannot convert to raw entity. " +
                $"The source must implement {nameof(IRawEntity)} or {nameof(IRawEntityConvertible)}"
            )
        };
}