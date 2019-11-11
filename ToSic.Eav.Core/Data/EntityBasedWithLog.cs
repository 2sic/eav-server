using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A strongly typed object which gets its data from an entity, and also logs what it does.
    /// </summary>
    [PublicApi]
    public abstract class EntityBasedWithLog: EntityBasedType, IHasLog
    {
        /// <summary>
        /// An entity based type which also logs what it does
        /// </summary>
        /// <param name="entity">entity which will be the foundation of this type</param>
        /// <param name="parentLog">parent log to chain</param>
        /// <param name="logName">Name for the logger</param>
        protected EntityBasedWithLog(IEntity entity, ILog parentLog, string logName) : base(entity)
        {
            Log = new Log(logName, parentLog);
        }

        /// <inheritdoc/>
        public ILog Log { get; }

    }
}
