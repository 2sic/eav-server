namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// This is the default implementation of a converter, which simply returns the object itself.
/// It ignores the options, as they are often not needed.
/// </summary>
public class ConvertToRawSelf: IConvertToRawEntity
{
    public IRawEntity? TryRawEntity<TSource>(TSource source, RawConvertOptions options)
        where TSource : class =>
        source as IRawEntity;
    
    public static ConvertToRawSelf Instance { get; } = new();
}

public class ConvertToRawFactory<TFactorySource>(Func<TFactorySource, RawConvertOptions, IRawEntity> factory): IConvertToRawEntity
{
    public IRawEntity? TryRawEntity<TSource>(TSource source, RawConvertOptions options)
        where TSource : class, TFactorySource =>
        factory(source, options);
}
