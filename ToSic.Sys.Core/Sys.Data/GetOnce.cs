using System.Runtime.CompilerServices;

namespace ToSic.Sys.Data;

// TODO: consider renaming to LazyValue (to make it clearer what it is) and move the namespace away from Sys.Data

/// <summary>
/// Simple helper class to use on object properties which should be generated once.
/// Similar to Lazy(), but the generator function is passed in on the Get() call, not on the constructor.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <remarks>
/// Important for properties which can also return null, because then checking for null won't work to determine if we already tried to retrieve it.
/// 
/// Constructor is empty.
///
/// In case you're wondering why we can't pass the generator in on the constructor:
/// Reason is that in most cases we need real objects in the generator function,
/// which doesn't work in a `static` context.
/// This means that if the = new GetOnce() is run on the private property
/// (which is the most common case) most generators can't be added.
///
/// Typical use cases:
/// 
/// 1. Properties which could validly return a null, in which case ?.Get() would run too often.
/// 2. Properties which must - in rare cases, be reset
/// 3. Properties which must check if they were already created (like Lazy())
/// 4. Properties which have non-nullable types like tuples, which can't self-detect if they are still empty
/// 5. Methods which behave like properties but as methods (typically to prevent serialization), so they don't have a backing `field`, needing additional code
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class GetOnce<TValue>
{
    /// <summary>
    /// Get the value. If not yet retrieved, use the generator function (but only once). 
    /// </summary>
    /// <param name="generator">Function which will generate the value on first use.</param>
    /// <returns></returns>
    public TValue? Get(Func<TValue> generator)
    {
        if (IsValueCreated)
            return Value;
        // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
        Value = generator();
        // Important: This must happen explicitly after the generator() - otherwise there is a risk of cyclic code which already assume
        // the value was created, while still inside the creation of the value.
        // So we would rather have a stack overflow and find the problem code, than to let the code assume the value was already made and null
        IsValueCreated = true;
        return Value;
    }
    
    /// <summary>
    /// Determines if value has been created.
    /// The name `IsValueCreated` is the same as in a Lazy() object
    /// </summary>
    public bool IsValueCreated { get; protected set; }
    protected TValue? Value;
}

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyGetAndReset<TValue>: GetOnce<TValue>
{
    /// <summary>
    /// Reset the state and value so it will be re-generated next time it's needed.
    /// </summary>
    public void Reset()
        => IsValueCreated = false;
    
    /// <summary>
    /// Reset the state and value so it will be re-generated next time it's needed.
    /// </summary>
    public void Reset(TValue newValue)
    {
        Value = newValue;
        IsValueCreated = true;
    }
}

[PrivateApi]
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