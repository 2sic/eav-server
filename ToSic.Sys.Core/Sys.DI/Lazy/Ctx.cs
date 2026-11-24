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
public class Ctx<TService>() where TService : class
{
    /// <summary>
    /// Initializes a new instance of the <c>Ctx</c> type with the specified service value.
    /// </summary>
    /// <param name="value">Service value to assign to <c>Value</c>.</param>
    private Ctx(TService value): this()
        => Value = value;

    private Ctx(bool isRegistered): this()
        => _isRegistered = isRegistered;

    private readonly bool _isRegistered;

    /// <summary>
    /// Gets the service value. If the service was registered but not assigned, throws <c>RegisteredButNotAssignedException</c>. If the service was not registered, throws <c>ArgumentNullException</c>.
    /// </summary>
    /// <exception cref="CtxRegisteredButNotAssignedException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public TService Value => field ?? (
        _isRegistered
            ? throw new CtxRegisteredButNotAssignedException()
            : throw new CtxNotRegisteredException());

    public static Ctx<TService> Transport(TService value) => new(value);


    public static Func<IServiceProvider, Ctx<TService>> Register() => OverrideService<Ctx<TService>>.Register(() => new(true));

}
internal class CtxNotRegisteredException()
    : Exception("Value was not registered - this is a bug, please report it.");

internal class CtxRegisteredButNotAssignedException()
    : Exception("Value was registered but never assigned - this is a bug, please report it.");
