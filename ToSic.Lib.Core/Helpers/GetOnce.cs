using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Helpers
{
    /// <summary>
    /// Simple helper class to use on object properties which should be generated once.
    /// 
    /// Important for properties which can also return null, because then checking for null won't work to determine if we already tried to retrieve it.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public class GetOnce<TResult>
    {
        /// <summary>
        /// Construct an empty GetOnce object for use later on.
        ///
        /// In case you're wondering why we can't pass the generator in on the constructor:
        /// Reason is that in most cases we need real objects in the generator function,
        /// which doesn't work in a `static` context.
        /// This means that if the = new GetOnce() is run on the private property
        /// (which is the most common case) most generators can't be added. 
        /// </summary>
        public GetOnce() {}

        /// <summary>
        /// Get the value. If not yet retrieved, use the generator function (but only once). 
        /// </summary>
        /// <param name="generator">Function which will generate the value on first use.</param>
        /// <returns></returns>
        public TResult Get(Func<TResult> generator)
        {
            if (IsValueCreated) return _value;
            // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
            _value = generator();
            // Important: This must happen explicitly after the generator() - otherwise there is a risk of cyclic code which already assume
            // the value was created, while still inside the creation of the value.
            // So we would rather have a stack overflow and find the problem code, than to let the code assume the value was already made and null
            IsValueCreated = true;
            return _value;
        }

        /// <summary>
        /// Getter with will log it's actions when it retrieves the property the first time.
        /// </summary>
        /// <param name="log">Log object to use when logging</param>
        /// <param name="generator">
        /// Function which will generate the value on first use.
        /// The function must return the expected value/type.
        /// </param>
        /// <param name="timer">enable a timer from call/close</param>
        /// <param name="enabled">can be set to false if you want to disable logging</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        /// <returns></returns>
        public TResult Get(ILog log, Func<TResult> generator,
            bool timer = default,
            bool enabled = true,
            string parameters = default,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            if (IsValueCreated) return _value;
            IsValueCreated = true;
            return _value = log.Getter(generator, timer: timer, enabled: enabled, parameters: parameters, message: message, cPath: cPath, cName: cName, cLine: cLine);
        }

        /// <summary>
        /// Getter with will log when it gets the property the first time.
        /// This will also provide the logger to the generator as a first function parameter.
        /// </summary>
        /// <param name="log">Log object to use when logging</param>
        /// <param name="generator">
        /// Function which will generate the value on first use.
        /// The function must return the expected value/type.
        /// </param>
        /// <param name="timer">enable a timer from call/close</param>
        /// <param name="enabled">can be set to false if you want to disable logging</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        /// <returns></returns>
        public TResult GetL(ILog log, Func<ILog, TResult> generator,
            bool timer = default,
            bool enabled = true,
            string parameters = default,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            if (IsValueCreated) return _value;
            IsValueCreated = true;
            return _value = log.GetterL(generator, timer: timer, enabled: enabled, parameters: parameters, message: message, cPath: cPath, cName: cName, cLine: cLine);
        }

        /// <summary>
        /// Getter with will log when it gets the property the first time.
        /// It provides the logger as a first function in the parameter.
        /// </summary>
        /// <param name="log">Log object to use when logging</param>
        /// <param name="generator">
        /// Function which will generate the value on first use.
        /// The function must return a Tuple `(TResult Result, string Message)` which it uses to log when done.
        /// </param>
        /// <param name="timer">enable a timer from call/close</param>
        /// <param name="enabled">can be set to false if you want to disable logging</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        /// <returns></returns>
        public TResult GetM(ILog log, Func<ILog, (TResult Result, string Message)> generator,
            bool timer = default,
            bool enabled = true,
            string parameters = default,
            string message = default,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            if (IsValueCreated) return _value;
            IsValueCreated = true;
            return _value = log.GetterM(generator, timer: timer, enabled: enabled, cPath: cPath, cName: cName, cLine: cLine);
        }

        /// <summary>
        /// Reset the state and value so it will be re-generated next time it's needed.
        /// </summary>
        public void Reset(ILog log = default)
        {
            log.A(nameof(Reset));
            IsValueCreated = false;
        }

        /// <summary>
        /// Reset the state and value so it will be re-generated next time it's needed.
        /// </summary>
        public void Reset(TResult newValue)
        {
            _value = newValue;;
            IsValueCreated = true;
        }

        /// <summary>
        /// Determines if value has been created.
        /// The name `IsValueCreated` is the same as in a Lazy() object
        /// </summary>
        public bool IsValueCreated { get; private set; }
        private TResult _value;
    }
}
