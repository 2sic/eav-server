using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public interface IAppIdentityWithPublishingState: IAppIdentity
    {
        [PrivateApi]
        bool ShowDraftsInData { get; }
        [PrivateApi]
        bool VersioningEnabled { get; }

    }
}
