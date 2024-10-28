using ToSic.Lib.Data;

namespace ToSic.Eav.StartUp;

/// <summary>
/// Defines a service (which must be added using AddTransient (not TryAddTransient).
/// Can then do more registrations at startup, like register features
/// </summary>
[PrivateApi]
public interface IStartUpRegistrations: IHasLog, IHasIdentityNameId
{
    void Register();
}