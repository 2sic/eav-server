﻿using ToSic.Eav;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;

namespace ToSic.Testing.Shared
{
    public static class DataSourceFactoryTestExtensions
    {
        public static TDataSource TestCreate<TDataSource>(
            this IDataSourceFactory dsf,
            string noParamOrder = Parameters.Protector,
            IDataSource upstream = default,
            IAppIdentity appIdentity = default,
            ILookUpEngine configLookUp = default) where TDataSource : IDataSource
            => dsf.Create<TDataSource>(links: upstream, options: new DataSourceOptions(appIdentity: appIdentity, lookUp: configLookUp));

        public static TDataSource TestCreateNew<TDataSource>(
            this IDataSourceFactory dsf,
            string noParamOrder = Parameters.Protector,
            IDataSource upstream = default,
            IAppIdentity appIdentity = default,
            object options = default) where TDataSource : IDataSource
            => dsf.Create<TDataSource>(links: upstream, options: new DataSourceOptions.Converter()
                .Create(new DataSourceOptions(appIdentity: appIdentity, lookUp: new LookUpEngine(null)), options));

    }
}
