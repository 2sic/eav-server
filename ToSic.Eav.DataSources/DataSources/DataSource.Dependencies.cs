using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        public class MyServices : MyServicesBase
        {
            public DataSourceConfiguration Configuration { get; }
            public ConfigurationDataLoader ConfigDataLoader { get; }
            public LazySvc<DataSourceErrorHelper> ErrorHandler { get; }

            /// <summary>
            /// Note that we will use Generators for safety, because in rare cases the dependencies could be re-used to create a sub-data-source
            /// </summary>
            public MyServices(
                DataSourceConfiguration configuration,
                LazySvc<DataSourceErrorHelper> errorHandler,
                ConfigurationDataLoader configDataLoader
            )
            {
                ConnectServices(
                    Configuration = configuration,
                    ErrorHandler = errorHandler,
                    ConfigDataLoader = configDataLoader
                );
            }
        }
    }
}
