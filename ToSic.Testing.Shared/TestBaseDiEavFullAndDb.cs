using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.Run;
using ToSic.Lib.Helpers;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Base class for tests providing all the Eav dependencies (Apps, etc.)
    /// </summary>
    public abstract class TestBaseDiEavFullAndDb: TestBaseEav
    {
        protected override void Configure()
        {
            base.Configure();

            // Make sure global data is loaded
            GetService<SystemLoader>().StartUp();
        }

        public DataSourceFactory DataSourceFactory => _dataSourceFactory.Get(GetService<DataSourceFactory>);
        private readonly GetOnce<DataSourceFactory> _dataSourceFactory = new GetOnce<DataSourceFactory>();



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
