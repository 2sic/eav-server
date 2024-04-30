using ToSic.Lib.Data;
using ToSic.Sxc.Apps;

namespace ToSic.Eav.Apps.Specs;

public interface IAppSpecs: IAppIdentity, IHasIdentityNameId
{
    string Name { get; }

    string Folder { get; }

    IAppConfiguration Configuration { get; }

}