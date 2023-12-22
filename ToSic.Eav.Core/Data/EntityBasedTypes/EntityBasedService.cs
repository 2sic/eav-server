using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Experimental type for a thing which needs services to work, but has all the data from a Entity.
/// </summary>
[PrivateApi("WIP v15")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class EntityBasedService<T>: EntityBasedWithLog where T : EntityBasedService<T>
{
    protected EntityBasedService(string logName) : base(null, null, logName ?? $"{EavLogs.Eav}.EntSrv")
    {
    }

    public T Init(IEntity entity)
    {
        Entity = entity;
        return this as T;
    }
}