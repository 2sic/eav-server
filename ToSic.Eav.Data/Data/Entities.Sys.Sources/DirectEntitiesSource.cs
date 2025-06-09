namespace ToSic.Eav.Data.Entities.Sys.Sources;

/// <summary>
/// An entities source which directly delivers the given entities
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class DirectEntitiesSource : IEntitiesSource
{
    public IEnumerable<IEntity> List { get; private set; }

    protected DirectEntitiesSource(IEnumerable<IEntity> entities)
    {
        List = entities;
    }

    public static TResult Using<TResult>(Func<(DirectEntitiesSource Source, List<IEntity> List), TResult> action)
    {
        var list = new List<IEntity>();
        var directEntitiesSource = new DirectEntitiesSource(list);
        var result = action((directEntitiesSource, list));
        directEntitiesSource.List = list.ToImmutableSafe();
        directEntitiesSource.CacheTimestamp = DateTime.Now.Ticks + 1; // just in case it's so fast that we would still get the same tick
        return result;
    }


    public long CacheTimestamp { get; private set; } = DateTime.Now.Ticks;

    /// <summary>
    /// Return false for cache changed to prevent reloading the cache unnecessarily
    /// </summary>
    public bool CacheChanged(long dependentTimeStamp) => CacheTimestamp > dependentTimeStamp;

}