using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.Run;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Base class for tests providing all the Eav dependencies (Apps, etc.)
    /// </summary>
    public abstract class TestBaseDiEavFullAndDb: TestBaseDiEmpty
    {
        protected override void Configure()
        {
            base.Configure();

            // Make sure global data is loaded
            Build<SystemLoader>().StartUp();
        }

        protected override IServiceCollection SetupServices(IServiceCollection services)
        {
            return base.SetupServices(services)
                .AddEav();
        }

        public DataSourceFactory DataSourceFactory =>
            _dataSourceFactory ?? (_dataSourceFactory = Build<DataSourceFactory>());
        private DataSourceFactory _dataSourceFactory;



        /// <summary>
        /// Use this helper when you have a stream, but for testing need only a subset of the items in it. 
        /// 
        /// Will use a EntityIdFilter to achieve this
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="inStream"></param>
        /// <returns></returns>
        protected IDataStream FilterStreamByIds(IEnumerable<int> ids, IDataStream inStream)
        {
            if (ids != null && ids.Any())
            {
                var entityFilterDs = DataSourceFactory.GetDataSource<EntityIdFilter>(inStream);
                entityFilterDs.EntityIds = string.Join(",", ids);
                inStream = entityFilterDs.GetStream();
            }

            return inStream;
        }
    }
}
