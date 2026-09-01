namespace ToSic.Sys.Caching;

/// <summary>
/// Special class to cache/buffer some information for the lifetime of the scope.
/// </summary>
/// <remarks>
/// Initially introduced to cache ToolbarButtonDecorators, but could be used for anything.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class ScopedCache<T> where T : class, new()
{
    public T Cache = new();
}
