using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    public static class IServiceBuilderExtensions
    {
        public static T GetTestDataSource<T>(this IServiceBuilder parent, ILookUpEngine lookUps = null) where T : IDataSource
        {
            var dsf = parent.GetService<DataSourceFactory>();
            var ds = dsf.GetDataSource<T>(new AppIdentity(0, 0), null, lookUps ?? LookUpTestData.EmptyLookupEngine);
            return ds;
        }

    }
}
