namespace ToSic.Eav.Data;

/// <summary>
/// Experimental type for a thing which needs services to work, but has all the data from a Entity.
/// </summary>
[PrivateApi("WIP v15")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class EntityBasedService<T>(string logName)
    : EntityBasedWithLog(null, null, logName ?? $"{EavLogs.Eav}.EntSrv")
    where T : EntityBasedService<T>
{
    public T Init(IEntity entity)
    {
        Entity = entity;
        return this as T;
    }
}