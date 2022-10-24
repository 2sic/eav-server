using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;

namespace ToSic.Eav.ImportExport.Json
{
    public abstract class JsonSerializerBase<T>: SerializerBase where T: class
    {
        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        protected JsonSerializerBase(ITargetTypes metadataTargets, IAppStates appStates, string logName) : base(metadataTargets, appStates, logName) { }

        public T Init(AppState package, ILog parentLog)
        {
            Initialize(package, parentLog);
            return this as T;
        }
    }
}
