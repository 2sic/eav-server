﻿using DotNetNuke.Entities.Modules;
using System;
using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironmentVersioning
    {
        bool Supported { get; }

        /// <summary>
        /// Get the current versioning requirements.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        VersioningRequirements Requirements(int moduleId);

        /// <summary>
        /// Wraps an action and performs pre/post processing related to versioning of the environment.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="userId"></param>
        /// <param name="action"></param>
        void DoInsideVersioning(int moduleId, int userId, Action<VersioningActionInfo> action);

        /// <summary>
        /// Wraps an action inside publish of latest version.
        /// NOTE: Should be called by the business-controller of the module. The controller must implement the IVersionable.
        /// </summary>
        /// <param name="moduleId"></param>
        void DoInsidePublishLatestVersion(int moduleId, Action<VersioningActionInfo> action);

        /// <summary>
        /// Wraps an action inside delete/discard of latest version.
        /// NOTE: Should be called by the business-controller of the module. The controller must implement the IVersionable.
        /// </summary>
        /// <param name="moduleId"></param>
        void DoInsideDeleteLatestVersion(int moduleId, Action<VersioningActionInfo> action);

        int GetLatestVersion(int moduleId);

        int GetPublishedVersion(int moduleId);
    }
}
