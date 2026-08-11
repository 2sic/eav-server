namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// Extensions for any <see cref="IRawData"/> but it must implement other interfaces to properly support conversion.
/// </summary>
[PrivateApi]
public static class IRawEntitySourceExtensions
{

    /// <summary>
    /// Extension method, to convert any <see cref="IRawData"/> to a <see cref="IRawEntity"/>
    /// </summary>
    public static IRawEntity GetRawFromConverterOrDirectCast<TSource>(this TSource source, RawConvertOptions options)
        where TSource : class, IRawData =>
        source switch
        {
            IRawEntityConvertible getConverter => getConverter
                .GetConverter()
                .Convert(source, options),
            
            IRawEntityAutoConvert autoConvert => RawFromAnonymousHelper
                .ConvertBasics(autoConvert),
            
            // This IRawEntity **must** come last, as some objects may implement both IRawEntityConvertible and IRawEntity, and we want to use the converter in that case.
            IRawEntity rawEntity => rawEntity,
            
            _ => throw new InvalidCastException(
                $"Cannot convert to raw entity. " +
                $"The source must implement {nameof(IRawEntity)} or {nameof(IRawEntityConvertible)}"
            )
        };
}
