using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps
{
    public interface IAppIdentityWithPublishingState: IAppIdentity
    {
        [PrivateApi]
        bool ShowDrafts { get; }
    }
}
