using System.Runtime.CompilerServices;

namespace ToSic.Sys.DI;

/// <summary>
/// Experimental class to allow for overriding a service in a specific context.
/// This is useful for testing or for scenarios where you want to temporarily replace a service with a different implementation.
/// </summary>
/// <remarks>
/// The service can be consumed anywhere, but will throw an error if no override was given (and <see cref="IsSet"/> is not checked first).
/// </remarks>
/// <typeparam name="TService"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class Ctx<TService>(ISysServiceProvider sp)
    where TService : class
{
    /// <summary>
    /// The factory function as it exists at the moment of creating this service
    /// </summary>
    private readonly Func<ISysServiceProvider, CtxValue<TService>>? _overrideFactoryAtCreation
        = OverrideService<CtxValue<TService>>.CurrentFactory;

    public bool IsSet => _overrideFactoryAtCreation != null;

    /// <summary>
    /// The value as provided by the override.
    /// </summary>
    /// <exception cref="CtxNotAssignedException"></exception>
    /// 
    public TService Value => field
        ??= ValueOrNull ?? throw new ArgumentNullException(nameof(Value));

    /// <summary>
    /// Can also return `null` - but will still throw exception if it was not overriden.
    /// </summary>
    public TService? ValueOrNull => field
        ??= _overrideFactoryAtCreation != null
            ? _overrideFactoryAtCreation(sp).Value
            : throw new CtxNotAssignedException(typeof(TService));

    /// <summary>
    /// The value - or null if it was not set.
    /// This is the only property that will not throw an exception if the value was not set.
    /// </summary>
    public TService? ValueOptional => field
        ??= _overrideFactoryAtCreation?.Invoke(sp).Value;

    /// <summary>
    /// Register a replacement for the context.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="cName"></param>
    /// <returns></returns>
    public static IDisposable Use(TService value, [CallerMemberName] string? cName = null)
        => OverrideService<CtxValue<TService>>.Use(new CtxValue<TService>(value), cName: cName);

    /// <summary>
    /// Register a replacement using a function - for delayed retrieval.
    /// </summary>
    /// <param name="valueFactory"></param>
    /// <param name="cName"></param>
    /// <returns></returns>
    public static IDisposable Use(Func<TService> valueFactory, [CallerMemberName] string? cName = null)
        => OverrideService<CtxValue<TService>>.Use((_) => new(valueFactory()), cName: cName);
}

internal class CtxNotAssignedException(Type ctxType)
    : Exception($"Value of type '{ctxType.Name}' was registered but never assigned - this is a bug, please report it.");

