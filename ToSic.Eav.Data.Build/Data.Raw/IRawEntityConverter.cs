namespace ToSic.Eav.Data.Raw;

/// <summary>
/// WIP v22 2dm
/// </summary>
[WorkInProgressApi("v22")]
public interface IRawEntityConverter
{
    IRawEntity Convert<TSource>(TSource source, RawConvertOptions options)
        where TSource : class;
}