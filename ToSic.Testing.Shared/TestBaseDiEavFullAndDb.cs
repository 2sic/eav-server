using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ToSic.Eav;
using ToSic.Eav.Configuration;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.DataSources;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Testing.Shared
{
    /// <summary>
    /// Base class for tests providing all the Eav dependencies (Apps, etc.)
    /// </summary>
    public abstract class TestBaseDiEavFullAndDb: TestBaseDiEmpty
    {
        protected TestBaseDiEavFullAndDb()
        {
            // this will run after the base constructor, which configures DI
            var dbConfiguration = Build<IDbConfiguration>();
            dbConfiguration.ConnectionString = DbConnectionString;

            var globalConfig = Build<IGlobalConfiguration>();
            globalConfig.DataFolder = "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";

            // Try to reset some special static variables which may cary over through many tests
            SystemFingerprint.ResetForTest();

            // Make sure global types are loaded
            Build<SystemLoader>().StartUp();
        }

        protected virtual string DbConnectionString => TestConstants.ConStr;

        protected override IServiceCollection SetupServices(IServiceCollection services = null)
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
