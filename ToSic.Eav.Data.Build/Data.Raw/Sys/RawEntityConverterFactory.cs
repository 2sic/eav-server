namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// This is the basic implementation of a converter, using a simple factory function
/// </summary>
public class RawEntityConverterFactory<TFactorySource>(Func<TFactorySource, RawConvertOptions, IRawEntity> factory)
    : IRawEntityConverter
    where TFactorySource : class
{
    public IRawEntity Convert<TSource>(TSource source, RawConvertOptions options)
        where TSource : class =>
        factory(source as TFactorySource
                ?? throw new InvalidOperationException($"Invalid source type: {source?.GetType().Name}, could not convert to {typeof(TFactorySource).Name}"),
            options
        );
}
