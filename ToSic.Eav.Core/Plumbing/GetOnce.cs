using System;
using ToSic.Lib.Logging;
using ToSic.Lib.Documentation;


namespace ToSic.Eav.Plumbing
{
    /// <summary>
    /// Simple helper class to use on object properties which should be generated once.
    /// 
    /// Important for properties which can also return null, because then checking for null won't work to determine if we already tried to retrieve it.
    ///
    /// ATM used in the ResponsiveBase, but we should also use it in other places where we have a second toggle to determine if it had been retrieved already. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PrivateApi("internal use only")]
    public class GetOnce<T>
    {
        /// <summary>
        /// Get the value once only. If not yet retrieved, use the generator function. 
        /// </summary>
        /// <param name="generator">Function which will generate the value on first use.</param>
        /// <returns></returns>
        public virtual T Get(Func<T> generator)
        {
            if (IsValueCreated) return _value;
            IsValueCreated = true;
            // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
            return _value = generator();
        }

        /// <summary>
        /// Get the value once only. If not yet retrieved, use the generator function.
        ///
        /// Also log what the returned value was for better insights.
        /// </summary>
        /// <param name="generator">Function which will generate the value on first use.</param>
        /// <param name="log">Logger to use</param>
        /// <param name="name">name of the property we're getting, to mention in the log</param>
        /// <returns></returns>
        public T Get(Func<T> generator, ILog log, string name)
        {
            var wrapLog = log.Fn<T>(name);
            var result = Get(generator);
            return wrapLog.ReturnAndLog(result);
        }

        /// <summary>
        /// Determines if value has been created. Name 'IsValueCreated' is the same as in the Lazy() object
        /// </summary>
        public bool IsValueCreated;
        private T _value;
    }
}
