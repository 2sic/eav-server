namespace ToSic.Eav.Data.Sys.ValueConverter;

public static class ValueConverterExtensions
{
    public static object PreConvertReferences(this IValueConverter valueConverter, object value, ValueTypes valueType, bool resolveHyperlink)
    {
        var l = (valueConverter as IHasLog)?.Log./*IfDetails(Options.LogSettings).*/Fn<object>();
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