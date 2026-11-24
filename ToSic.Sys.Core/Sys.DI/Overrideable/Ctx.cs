using System.Runtime.CompilerServices;

namespace ToSic.Sys.DI;

/// <summary>
/// Experimental class to allow for overriding a service in a specific context.
/// This is useful for testing or for scenarios where you want to temporarily replace a service with a different implementation.
/// </summary>
/// <remarks>
/// To even allow this, it must initially 
/// </remarks>
/// <typeparam name="TService"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
//public class Ctx<TService>() where TService : class
//{
//    /// <summary>
//    /// Initializes a new instance of the <c>Ctx</c> type with the specified service value.
//    /// </summary>
//    /// <param name="value">Service value to assign to <c>Value</c>.</param>
//    private Ctx(TService value): this()
//        => Value = value;

//    private Ctx(bool isRegistered): this()
//        => _isRegistered = isRegistered;

//    private readonly bool _isRegistered;

//    /// <summary>
//    /// Gets the service value. If the service was registered but not assigned, throws <c>RegisteredButNotAssignedException</c>. If the service was not registered, throws <c>ArgumentNullException</c>.
//    /// </summary>
//    /// <exception cref="CtxRegisteredButNotAssignedException"></exception>
//    /// <exception cref="ArgumentNullException"></exception>
//    public TService Value => field ?? (
//        _isRegistered
//            ? throw new CtxRegisteredButNotAssignedException(GetType())
//            : throw new CtxNotRegisteredException());

//    public static Ctx<TService> Transport(TService value) => new(value);


//    public static Func<IServiceProvider, Ctx<TService>> Register() => OverrideService<Ctx<TService>>.Register(() => new(true));

//}

// V2
//public sealed class Ctx<TService>(ISysServiceProvider sp) where TService : class
//{
//    private Ctx(bool set, TService value) : this(null!)
//        => _value = value;


//    private readonly TService? _value = OverrideService<TService>.CurrentFactory is { } factory
//        ? factory(sp)
//        : sp.GetService(typeof(TService)) as TService;

//    public bool HasValue => _value is not null;

//    public TService Value => _value ?? throw new CtxRegisteredButNotAssignedException(GetType());

//    public TService? ValueOrDefault => _value;

//    public static Ctx<TService> Transport(TService value) => new(true, value);

//    public static implicit operator TService(Ctx<TService> ctx) => ctx.Value;
//}

// V3
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
    /// Register a replacement for the context.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="hint"></param>
    /// <param name="cName"></param>
    /// <returns></returns>
    public static IDisposable Use(TService value, string? hint = null, [CallerMemberName] string? cName = null)
        => OverrideService<CtxValue<TService>>.Use(new CtxValue<TService>(value), hint: hint, cName: cName);

}

internal class CtxNotAssignedException(Type ctxType)
    : Exception($"Value of type '{ctxType.Name}' was registered but never assigned - this is a bug, please report it.");

