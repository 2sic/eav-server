namespace ToSic.Eav.Apps;

/// <summary>
/// A special AppIdentity which is pure and does not have any other data attached.
/// This is meant for objects which clearly show that they really just want the Identity and nothing else.
/// </summary>
public sealed class AppIdentityPure: AppIdentity, IAppIdentityPure
{
    public AppIdentityPure(int zoneId, int appId) : base(zoneId, appId)
    {
    }

    public AppIdentityPure(IAppIdentity parent) : base(parent)
    {
    }
}