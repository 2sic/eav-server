namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// This is the basic implementation of a converter, using a simple factory function
/// </summary>
public class RawEntityConverterFactory<TData>(Func<TData, RawConvertOptions, IRawEntity> factory)
    : IRawEntityConverter
    where TData : class
{
    public IRawEntity Convert<TSource>(TSource source, RawConvertOptions options)
        where TSource : class =>
        factory(source as TData
                ?? throw new InvalidOperationException($"Invalid source type: {source?.GetType().Name}, could not convert to {typeof(TData).Name}"),
            options
        );
}
