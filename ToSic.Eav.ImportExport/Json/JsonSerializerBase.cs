using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Json
{
    public abstract class JsonSerializerBase<T>: SerializerBase where T: class
    {
        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        protected JsonSerializerBase(ITargetTypes metadataTargets, IGlobalTypes globalTypes, string logName) : base(metadataTargets, globalTypes, logName) { }

        public T Init(AppState package, ILog parentLog)
        {
            Initialize(package, parentLog);
            return this as T;
        }
    }
}
