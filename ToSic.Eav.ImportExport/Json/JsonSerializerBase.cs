using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport.Json
{
    public abstract class JsonSerializerBase<T>: SerializerBase where T: class
    {
        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        protected JsonSerializerBase(string logName) : base(logName) { }

        public T Init(AppState package, ILog parentLog)
        {
            Initialize(package, parentLog);
            return this as T;
        }
    }
}
