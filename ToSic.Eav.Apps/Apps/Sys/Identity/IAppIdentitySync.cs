namespace ToSic.Eav.Apps.Sys;

/// <summary>
/// Internal API to mark objects that can be updated with an AppIdentity
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppIdentitySync
{
    void UpdateAppIdentity(IAppIdentity appIdentity);
}