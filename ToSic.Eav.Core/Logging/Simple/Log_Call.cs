using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        public Action<string> Call(string methodName, string @params = null, string message = null, bool useTimer = false)
            => Wrapper($"{methodName}({@params}) {message}", useTimer);

        /// <summary>
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        public Action<string> Call(string methodName, Func<string> @params, Func<string> message = null, bool useTimer = false)
            => Call(methodName, Try(@params), message != null ? Try(message) : null, useTimer);


        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        public Func<string, T, T> Call<T>(string methodName, string @params = null, string message = null, bool useTimer = false)
            => Wrapper<T>($"{methodName}({@params}) {message}", useTimer);

    }
}
