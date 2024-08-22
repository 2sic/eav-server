namespace ToSic.Eav.Apps;

/// <summary>
/// This is used for functions which clearly only expect the identity,
/// and certainly no other / richer objects.
/// </summary>
public interface IAppIdentityPure: IAppIdentity;