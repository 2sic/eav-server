namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Internal API to mark objects that can be updated with an AppIdentity
    /// </summary>
    public interface IAppIdentitySync
    {
        void UpdateAppIdentity(IAppIdentity appIdentity);
    }
}
