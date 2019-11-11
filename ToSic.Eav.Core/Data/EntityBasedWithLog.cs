using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public abstract class EntityBasedWithLog: EntityBasedType, IHasLog
    {
        protected EntityBasedWithLog(IEntity entity, ILog parentLog, string logName) : base(entity)
        {
            Log = new Log(logName, parentLog);
        }

        public ILog Log { get; }

    }
}
