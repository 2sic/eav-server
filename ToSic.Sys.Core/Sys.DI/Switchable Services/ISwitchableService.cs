namespace ToSic.Sys.DI;

/// <summary>
/// This describes a service which may have multiple implementations, and will be picked by the <see cref="ServiceSwitcher{T}"/>
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISwitchableService : IHasIdentityNameId
{
    bool IsViable();

    int Priority { get; }
}