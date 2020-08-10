﻿using System;
using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Environment
{
    /// <summary>
    /// This is the fallback page publishing strategy, which basically says that page publishing isn't enabled
    /// NOTE: It is currently not in use, and that's ok. 
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class NoPagePublishing : IPagePublishing
    {
        public bool Supported => false;

        public PublishingMode Requirements(int moduleId)
        {
            return PublishingMode.DraftOptional;
        }

        public void DoInsidePublishing(int moduleId, int userId, Action<VersioningActionInfo> action)
        {
            var info = new VersioningActionInfo();
            action.Invoke(info);
        }

        public void DoInsidePublishLatestVersion(int moduleId, Action<VersioningActionInfo> action)
        {
            // NOTE: Do nothing!
        }

        public void DoInsideDeleteLatestVersion(int moduleId, Action<VersioningActionInfo> action)
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

        public void Publish(int instanceId, int version)
        {
            // ignore
        }

        public bool IsEnabled(int instanceId)
        {
            throw new NotImplementedException();
        }

        public IPagePublishing Init(ILog parent)
        {
            return this;
        }
    }
}
