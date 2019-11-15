using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public interface IAppIdentityWithPublishingState: IInAppAndZone
    {
        [PrivateApi]
        bool ShowDrafts { get; }
        [PrivateApi]
        bool EnablePublishing { get; }

    }
}
