using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="generate"></param>
        /// <returns></returns>
        public T Intercept<T>(string message, Func<T> generate)
        {
            var result = generate();
            var e = AddEntry($"{message}");
            e.AppendResult($"{result}");
            return result;
        }
    }
}
