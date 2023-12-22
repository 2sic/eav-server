using ToSic.Lib.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.StartUp;

/// <summary>
/// Defines a service (which must be added using AddTransient (not TryAddTransient).
/// Can then do more registrations at startup, eg. register features
/// </summary>
[PrivateApi]
public interface IStartUpRegistrations: IHasLog, IHasIdentityNameId
{
    void Register();
}