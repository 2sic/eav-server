using ToSic.Eav.Apps;

namespace ToSic.Eav.Context;

public interface IContextOfAppFactory
{
    IContextOfApp Create(IAppIdentity? appIdentity);
    IContextOfApp Create(int appId);
}