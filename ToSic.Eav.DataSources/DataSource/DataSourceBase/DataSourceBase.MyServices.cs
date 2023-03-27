using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSource
{
    public abstract partial class DataSourceBase
    {
        /// <summary>
        /// Services used by the <see cref="DataSourceBase"/>.
        /// This ensures that it's easy to inherit DataSources, while giving it all the services it needs even if the needs change with time.
        /// </summary>
        /// <remarks>
        /// * Added in v15.0x
        /// * Important: The internals of this class are not documented, as they will change with time.
        /// </remarks>
        [InternalApi_DoNotUse_MayChangeWithoutNotice]
        public class MyServices : MyServicesBase
        {
            [PrivateApi] public DataSourceConfiguration Configuration { get; }
            [PrivateApi] public ConfigurationDataLoader ConfigDataLoader { get; }
            [PrivateApi] public LazySvc<DataSourceErrorHelper> ErrorHandler { get; }

            /// <summary>
            /// Note that we will use Generators for safety, because in rare cases the dependencies could be re-used to create a sub-data-source
            /// </summary>
            [PrivateApi]
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
