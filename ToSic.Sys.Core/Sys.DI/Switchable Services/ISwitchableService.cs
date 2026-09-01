namespace ToSic.Sys.DI;

/// <summary>
/// This describes a service which may have multiple implementations, and will be picked by the <see cref="ServiceSwitcher{T}"/>
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISwitchableService : IHasIdentityNameId
{
    /// <summary>
    /// Will tell the service switcher if it should be used.
    /// Implementations will be cycled through according to priority.
    /// </summary>
    /// <returns></returns>
    bool IsViable();

    /// <summary>
    /// Priority, sorted Descending (higher is better).
    /// This is used to pick the best service when multiple are viable.
    /// </summary>
    int Priority { get; }
}