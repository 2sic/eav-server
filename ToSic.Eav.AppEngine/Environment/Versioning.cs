using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using System;
using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Environment
{
    public class Versioning : IEnvironmentVersioning
    {
        public bool Supported => false;

        public VersioningRequirements Requirements(int moduleId)
        {
            return VersioningRequirements.DraftOptional;
        }

        public void DoInsideVersioning(int moduleId, int userId, Action<VersioningActionInfo> action)
        {
            var info = new VersioningActionInfo();
            action.Invoke(info);
        }

        public void PublishLatestVersion(int moduleId)
        {
            // NOTE: Do nothing!
        }

        public void DeleteLatestVersion(int moduleId)
        {
            // NOTE: Do nothing!
        }

        public int GetLatestVersion(int moduleId)
        {
            return 0;
        }

        public int GetPublishedVersion(int moduleId)
        {
            return 0;
        }
    }
}
