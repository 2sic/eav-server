using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.AppSys
{
    public class AppContentTypes: ServiceBase
    {
        public AppContentTypes() : base("ApS.CtRead")
        {
        }

        public IEnumerable<IContentType> All(IAppWorkCtx appCtx) => appCtx.AppState.ContentTypes;

        /// <summary>
        /// Gets a ContentType by Name
        /// </summary>
        /// <returns>a content-type or null if not found</returns>
        public IContentType Get(IAppWorkCtx context, string name) => context.AppState.GetContentType(name);

    }
}
