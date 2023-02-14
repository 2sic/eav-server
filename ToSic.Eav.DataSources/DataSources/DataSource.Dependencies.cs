using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        public class Dependencies : ServiceDependencies
        {
            public DataSourceConfiguration Configuration { get; }
            public ConfigurationDataLoader ConfigDataLoader { get; }
            public LazySvc<DataSourceErrorHandling> ErrorHandler { get; }

            /// <summary>
            /// Note that we will use Generators for safety, because in rare cases the dependencies could be re-used to create a sub-data-source
            /// </summary>
            public Dependencies(
                DataSourceConfiguration configuration,
                LazySvc<DataSourceErrorHandling> errorHandler,
                ConfigurationDataLoader configDataLoader
            )
            {
                AddToLogQueue(
                    Configuration = configuration,
                    ErrorHandler = errorHandler,
                    ConfigDataLoader = configDataLoader
                );
            }
        }
    }
}
