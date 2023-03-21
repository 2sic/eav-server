using System;
using System.Collections.Generic;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources.Catalog
{
    public class AppDataSourcesLoaderUnknown : ServiceBase, IIsUnknown, IAppDataSourcesLoader
    {
        public AppDataSourcesLoaderUnknown(WarnUseOfUnknown<AppDataSourcesLoaderUnknown> _) : base("Eav.AppDtaSrcLoadUnk")
        { }

        public List<DataSourceInfo> Get(int appId)
        {
            throw new NotImplementedException();
        }
    }
}
