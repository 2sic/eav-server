﻿using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders
{
    public class AppContentTypesLoaderUnknown: ServiceBase, IAppContentTypesLoader, IIsUnknown
    {
        public AppContentTypesLoaderUnknown(WarnUseOfUnknown<AppContentTypesLoaderUnknown> _) : base(LogScopes.NotImplemented + ".RepLdr") { }

        public IAppContentTypesLoader Init(AppState app)
        {
            Log.A("Unknown App Repo loader - won't load anything");
            return this;
        }

        public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource) => new List<IContentType>();
    }
}