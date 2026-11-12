namespace ToSic.Eav.Data.Sys.ValueConverter;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ValueConverterExtensions
{
    /// <summary>
    /// Prepare a value for import, using the converter's logger.
    /// </summary>
    public static object PreConvertReferences(this IValueConverter valueConverter, object value, ValueTypes valueType, bool resolveHyperlink)
        => valueConverter.PreConvertReferences(value, valueType, resolveHyperlink, (valueConverter as IHasLog)?.Log);

    /// <summary>
    /// Prepare a value for import, using a caller-filtered logger for this operation.
    /// A null logger disables these diagnostic entries without changing the converter's logger.
    /// </summary>
    public static object PreConvertReferences(this IValueConverter valueConverter, object value, ValueTypes valueType, bool resolveHyperlink, ILog? log)
    {
        var l = log.Fn<object>();
        if (value is IAttribute)
            throw new ArgumentException($"Value must be a simple value but it's an {nameof(IAttribute)}");
        if (resolveHyperlink && valueType == ValueTypes.Hyperlink && value is string stringValue)
        {
            var converted = valueConverter.ToReference(stringValue);
            return l.Return(converted, $"Resolve hyperlink for '{stringValue}' - New value: '{converted}'");
        }
        return l.Return(value, "unmodified");
    }
    
}