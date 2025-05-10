using ToSic.Eav.Apps;

namespace ToSic.Eav.Context.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IContextResolver: IHasLog, IContextResolverUserPermissions
{
    /// <summary>
    /// This is the most basic kind of context. ATM you could also inject it directly,
    /// but we want to introduce the capability of giving a static site or something
    /// without having to write code implementing IContextOfSite
    /// </summary>
    /// <returns></returns>
    IContextOfSite Site();

    IContextOfApp SetApp(IAppIdentity appIdentity);

    IContextOfApp AppRequired();

    IContextOfApp AppOrNull();

}