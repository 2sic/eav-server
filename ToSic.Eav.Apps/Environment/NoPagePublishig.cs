using System;
using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Environment
{
    /// <summary>
    /// This is the fallback page publishing strategy, which basically says that page publishing isn't enabled
    /// NOTE: It is currently not in use, and that's ok. 
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class NoPagePublishing : HasLog<IPagePublishing>, IPagePublishing, IPagePublishingResolver
    {
        #region Constructors

        public NoPagePublishing() : base("Eav.NoPubl") { }

        IPagePublishingResolver IHasLog<IPagePublishingResolver>.Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }

        #endregion

        public bool Supported => false;

        public PublishingMode Requirements(int instanceId)
        {
            return PublishingMode.DraftOptional;
        }

        public void DoInsidePublishing(IInstanceContext context, Action<VersioningActionInfo> action)
        {
            var info = new VersioningActionInfo();
            action.Invoke(info);
        }

        //public void DoInsidePublishLatestVersion(int instanceId, Action<VersioningActionInfo> action)
        //{
        //    // NOTE: Do nothing!
        //}

        //public void DoInsideDeleteLatestVersion(int instanceId, Action<VersioningActionInfo> action)
        //{
        //    // NOTE: Do nothing!
        //}

        public int GetLatestVersion(int instanceId)
        {
            return 0;
        }

        public int GetPublishedVersion(int instanceId)
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

    }
}
