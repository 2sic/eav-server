using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Helper
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
    public class GetOnce<T>: IHasLog
    {
        public GetOnce() {}

        /// <summary>
        /// WIP experimental, not sure if this second way to use is a good idea
        /// </summary>
        public GetOnce(Func<T> generator) => _generator = generator;
        private readonly Func<T> _generator;

        /// <summary>
        /// WIP experimental, not sure if this second way to use is a good idea or causes confusion
        /// </summary>
        public T Value => Get(_generator 
                              ??
                              throw new Exception($"Can't use {nameof(Value)} if the generator wasn't set in the constructor"));

        /// <summary>
        /// Get the value once only. If not yet retrieved, use the generator function. 
        /// </summary>
        /// <param name="generator">Function which will generate the value on first use.</param>
        /// <returns></returns>
        public T Get(Func<T> generator)
        {
            if (IsValueCreated) return _value;
            IsValueCreated = true;
            // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
            return _value = generator();
        }

        /// <summary>
        /// EXPERIMENTAL - getter with will log when it gets the property the first time
        /// </summary>
        /// <param name="log"></param>
        /// <param name="generator"></param>
        /// <param name="cPath"></param>
        /// <param name="cName"></param>
        /// <param name="cLine"></param>
        /// <returns></returns>
        public T Get(ILog log, Func<T> generator,
            bool timer = default,
            bool enabled = true,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            if (IsValueCreated) return _value;
            IsValueCreated = true;
            return _value = log.Getter(generator, timer: timer, enabled: enabled, cPath: cPath, cName: cName, cLine: cLine);
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
        /// Reset the value so it will be re-generated next time it's needed
        /// </summary>
        public void Reset() => IsValueCreated = false;

        /// <summary>
        /// Determines if value has been created. Name 'IsValueCreated' is the same as in the Lazy() object
        /// </summary>
        public bool IsValueCreated { get; private set; }
        private T _value;

        public ILog Log { get; private set; }
    }
}
