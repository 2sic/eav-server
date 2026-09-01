using System.Runtime.CompilerServices;

namespace ToSic.Sys;

/// <summary>
/// A <see cref="LazyGet{TValue}"/> with logging.
/// </summary>
/// <typeparam name="TValue"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyGetAndLog<TValue>
{
    /// <summary>
    /// Getter with will log its actions when it retrieves the property the first time.
    /// </summary>
    /// <param name="log">Log object to use when logging</param>
    /// <param name="generator">
    /// Function which will generate the value on first use.
    /// The function must return the expected value/type.
    /// </param>
    /// <param name="timer">enable a timer from call/close</param>
    /// <param name="enabled">can be set to false if you want to disable logging</param>
    /// <param name="message"></param>
    /// <param name="cPath">auto pre-filled by the compiler - the path to the code file</param>
    /// <param name="cName">auto pre-filled by the compiler - the method name</param>
    /// <param name="cLine">auto pre-filled by the compiler - the code line</param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public TValue? Get(ILog log, Func<TValue> generator,
        bool timer = default,
        bool enabled = true,
        string? parameters = default,
        string? message = default,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    )
    {
        if (IsValueCreated)
            return Value;
        IsValueCreated = true;
        // ReSharper disable ExplicitCallerInfoArgument
        return Value = log.Getter(generator, timer: timer, enabled: enabled, parameters: parameters, message: message, cPath: cPath, cName: cName, cLine: cLine);
        // ReSharper restore ExplicitCallerInfoArgument
    }

    /// <summary>
    /// Determines if value has been created.
    /// The name `IsValueCreated` is the same as in a Lazy() object
    /// </summary>
    public bool IsValueCreated { get; protected set; }
    protected TValue? Value;
}