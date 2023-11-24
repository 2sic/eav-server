using ToSic.Eav.Apps;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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

    IContextOfApp App();

}