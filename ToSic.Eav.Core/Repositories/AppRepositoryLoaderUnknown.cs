using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Repositories
{
    public class AppRepositoryLoaderUnknown: HasLog, IAppRepositoryLoader, IIsUnknown
    {
        public AppRepositoryLoaderUnknown() : base(LogNames.NotImplemented + ".RepLdr") { }

        public IAppRepositoryLoader Init(int appId, string path, ILog log)
        {
            log.LinkTo(log);
            Log.Add("Unknown App Repo loader - won't load anything");
            return this;
        }

        public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource) => new List<IContentType>();
    }
}
