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
            public ILazySvc<IDataBuilder> DataBuilder { get; }
            public ILazySvc<DataSourceErrorHandling> ErrorHandler { get; }

            /// <summary>
            /// Note that we will use Generators for safety, because in rare cases the dependencies could be re-used to create a sub-data-source
            /// </summary>
            public Dependencies(
                ILazySvc<IDataBuilder> dataBuilder,
                ILazySvc<DataSourceErrorHandling> errorHandler,
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
