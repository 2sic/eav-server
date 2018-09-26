using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <summary>
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        public Action<string> New(string className, string @params = null, string message = null)
            => Wrapper($"new {className}({@params}) {message}", $"{className}() ");

        /// <summary>
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        public Action<string> New(string className, Func<string> @params, Func<string> message = null)
            => New(className, Try(@params), message != null ? Try(message) : null);

    }
}
