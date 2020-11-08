using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public interface IPagePublishingResolver: IHasLog<IPagePublishingResolver>
    {
        /// <summary>
        /// Informs if page publishing workflows are supported or not
        /// </summary>
        bool Supported { get; }

        /// <summary>
        /// Get the current versioning requirements. - to determine if it's required, optional etc.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        PublishingMode Requirements(int instanceId);

        bool IsEnabled(int instanceId);

    }
}
