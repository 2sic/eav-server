﻿using System;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSource
{
    /// <inheritdoc />
    /// <summary>
    /// Base DataSource class for providing data from external sources.
    /// This is the advanced base class which is more complex.
    /// You will usually want to use the CustomDataSourceLight
    /// </summary>
    /// <remarks>
    /// This has changed a lot in v15 (breaking change).
    /// Read about it in the docs.
    /// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract class CustomDataSourceAdvanced: DataSources.DataSource
    {

        public new class MyServices: DataSources.DataSource.MyServices
        {
            public IDataFactory DataFactory { get; }

            public MyServices(
                DataSourceConfiguration configuration,
                LazySvc<DataSourceErrorHelper> errorHandler,
                ConfigurationDataLoader configDataLoader,
                IDataFactory dataFactory) : base(configuration, errorHandler, configDataLoader)
            {
                ConnectServices(
                    DataFactory = dataFactory
                );
            }
        }

        /// <summary>
        /// Initializes an DataSource which will usually provide/generate external data.
        /// </summary>
        /// <param name="services">Dependencies needed by this data source and/or the parent</param>
        /// <param name="logName">
        /// The log name/identifier for insights logging.
        /// Optional, but makes debugging a bit easier when provided.
        /// </param>
        /// <remarks>
        /// set the cache creation date to the moment the object is constructed
        /// this is important, because the date should stay fixed throughout the lifetime of this object
        /// but renew when it is updates
        /// </remarks>
        protected CustomDataSourceAdvanced(MyServices services, string logName = null) : base(services, logName ?? $"{DataSourceConstants.LogPrefix}.Extern")
        {
            DataFactory = services.DataFactory;
        }
        protected CustomDataSourceAdvanced(MyServicesBase<MyServices> services, string logName = null) : base(services.ParentServices, logName ?? $"{DataSourceConstants.LogPrefix}.Extern")
        {
            ConnectServices(services);
            DataFactory = services.ParentServices.DataFactory;
        }

        /// <inheritdoc />
        public override long CacheTimestamp { get; } = DateTime.Now.Ticks;  // Initialize with moment the object was created

        protected IDataFactory DataFactory { get; }
    }
}