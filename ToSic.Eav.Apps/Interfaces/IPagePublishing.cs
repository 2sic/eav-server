using System;
using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    // Note: maybe some day this should go into a .Cms namespace
    [PrivateApi]
    public interface IPagePublishing: IHasLog<IPagePublishing>
    {
        bool Supported { get; }

        /// <summary>
        /// Get the current versioning requirements. - to determine if it's required, optional etc.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        PublishingMode Requirements(int moduleId);

        /// <summary>
        /// Wraps an action and performs pre/post processing related to versioning of the environment.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="userId"></param>
        /// <param name="action"></param>
        void DoInsidePublishing(int moduleId, int userId, Action<VersioningActionInfo> action);

        ///// <summary>
        ///// Wraps an action inside publish of latest version.
        ///// NOTE: Should be called by the business-controller of the module. The controller must implement the IVersionable.
        ///// </summary>
        ///// <param name="moduleId"></param>
        //void DoInsidePublishLatestVersion(int moduleId, Action<VersioningActionInfo> action);

        ///// <summary>
        ///// Wraps an action inside delete/discard of latest version.
        ///// NOTE: Should be called by the business-controller of the module. The controller must implement the IVersionable.
        ///// </summary>
        ///// <param name="moduleId"></param>
        //void DoInsideDeleteLatestVersion(int moduleId, Action<VersioningActionInfo> action);

        int GetLatestVersion(int moduleId);

        int GetPublishedVersion(int moduleId);

        void Publish(int instanceId, int version);

        bool IsEnabled(int instanceId);
    }
}
