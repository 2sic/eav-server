namespace ToSic.Sys.DI;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISwitchableService : IHasIdentityNameId
{
    bool IsViable();

    int Priority { get; }
}