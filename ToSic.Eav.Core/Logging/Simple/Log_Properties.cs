using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        public Action<string> Get(string property)
            => Wrapper($"get {property} start", $"get {property} done ");
        public Action<string> Set(string property)
            => Wrapper($"set {property} start", $"set {property} done ");

    }
}
