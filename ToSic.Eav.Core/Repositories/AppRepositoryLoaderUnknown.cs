using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Repositories
{
    public class AppRepositoryLoaderUnknown: HasLog, IAppRepositoryLoader, IIsUnknown
    {
        public AppRepositoryLoaderUnknown(WarnUseOfUnknown<AppRepositoryLoaderUnknown> warn) : base(LogNames.NotImplemented + ".RepLdr") { }

        public IAppRepositoryLoader Init(AppState app, ILog log)
        {
            log.LinkTo(log);
            Log.A("Unknown App Repo loader - won't load anything");
            return this;
        }

        public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource) => new List<IContentType>();
    }
}
