using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Experimental type for a thing which needs services to work, but has all the data from a Entity.
    /// </summary>
    [PrivateApi("WIP v15")]
    public abstract class EntityBasedService<T>: EntityBasedWithLog where T : EntityBasedService<T>
    {
        protected EntityBasedService(string logName) : base(null, null, logName ?? $"{LogNames.Eav}.EntSrv")
        {
        }

        public T Init(IEntity entity)
        {
            Entity = entity;
            return this as T;
        }
    }
}
