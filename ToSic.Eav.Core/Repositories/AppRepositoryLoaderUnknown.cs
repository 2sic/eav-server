using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.Services;

namespace ToSic.Eav.Repositories
{
    public class AppRepositoryLoaderUnknown: ServiceBase, IAppRepositoryLoader, IIsUnknown
    {
        public AppRepositoryLoaderUnknown(WarnUseOfUnknown<AppRepositoryLoaderUnknown> _) : base(LogScopes.NotImplemented + ".RepLdr") { }

        public IAppRepositoryLoader Init(AppState app)
        {
            Log.A("Unknown App Repo loader - won't load anything");
            return this;
        }

        public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource) => new List<IContentType>();
    }
}
