using ToSic.Eav.Data;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        public class Dependencies : ServiceDependencies
        {
            public DataSourceConfiguration.Dependencies ConfigDependencies { get; }
            public LazySvc<IDataBuilderInternal> DataBuilder { get; }
            public LazySvc<DataSourceErrorHandling> ErrorHandler { get; }

            /// <summary>
            /// Note that we will use Generators for safety, because in rare cases the dependencies could be re-used to create a sub-data-source
            /// </summary>
            public Dependencies(
                LazySvc<IDataBuilderInternal> dataBuilder,
                LazySvc<DataSourceErrorHandling> errorHandler,
                DataSourceConfiguration.Dependencies configDependencies
            )
            {
                AddToLogQueue(
                    DataBuilder = dataBuilder,
                    ErrorHandler = errorHandler,
                    ConfigDependencies = configDependencies
                );
            }
        }
    }
}
