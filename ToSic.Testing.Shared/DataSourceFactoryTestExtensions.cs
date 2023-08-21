﻿using ToSic.Eav;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSource;
using ToSic.Eav.LookUp;
using ToSic.Eav.Services;

namespace ToSic.Testing.Shared
{
    public static class DataSourceFactoryTestExtensions
    {
        public static TDataSource TestCreate<TDataSource>(
            this IDataSourcesService dsf,
            string noParamOrder = Parameters.Protector,
            IDataSource upstream = default,
            IAppIdentity appIdentity = default,
            ILookUpEngine configLookUp = default) where TDataSource : IDataSource
            => dsf.Create<TDataSource>(attach: upstream, options: new DataSourceOptions(appIdentity: appIdentity, lookUp: configLookUp));

        public static TDataSource TestCreateNew<TDataSource>(
            this IDataSourcesService dsf,
            string noParamOrder = Parameters.Protector,
            IDataSource upstream = default,
            IAppIdentity appIdentity = default,
            object options = default) where TDataSource : IDataSource
            => dsf.Create<TDataSource>(attach: upstream, options: new DataSourceOptions.Converter()
                .Create(new DataSourceOptions(appIdentity: appIdentity, lookUp: new LookUpEngine(null)), options));

    }
}
