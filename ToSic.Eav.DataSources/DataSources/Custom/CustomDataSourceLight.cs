﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Very lightweight DataSource base for data sources which are very simple and convention based.
    /// </summary>
    public abstract class CustomDataSourceLight: CustomDataSource
    {
        protected CustomDataSourceLight(MyServices services,
            string noParamOrder = Parameters.Protector,
            string logName = null) : base(services, logName ?? "Ds.CustLt")
        {
            Provide(() => GetRaw(() => new List<IRawEntity>(), null));
        }

        protected virtual DataFactoryOptions Options
        {
            get => _options ?? (_options = new DataFactoryOptions(typeName: "Custom"));
            set => _options = value;
        }
        private DataFactoryOptions _options;

        protected void ProvideOut<T>(
            IEnumerable<IHasRawEntity<T>> source,
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default) where T : IRawEntity =>
            Provide(() => GetHasRaw(() => source, options));

        protected void ProvideOut<T>(
            Func<IEnumerable<IHasRawEntity<T>>> source,
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default) where T : IRawEntity =>
            Provide(() => GetHasRaw(source, options));

        protected void ProvideOut<T>(
            IEnumerable<T> source,
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default) where T : IRawEntity => 
            ProvideOut(() => source, options: options);

        protected void ProvideOut<T>(
            Func<IEnumerable<T>> source,
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default) where T : IRawEntity =>
            Provide(() => GetRaw(source, options));

        private IImmutableList<IEntity> GetRaw<T>(Func<IEnumerable<T>> source, DataFactoryOptions options) where T: IRawEntity
        {
            var l = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();

            // Get raw entities - from _source or from override method
            var raw = source?.Invoke()?.ToList();

            // If we didn't get anything, return empty
            if (raw.SafeNone())
                return l.Return(DataSourceConstants.EmptyList, "no items returned");

            // Transform result to IEntity
            var result = DataFactory.New(options: options ?? Options).Create(raw);
            return l.Return(result, $"Got {result.Count} items");
        }

        private IImmutableList<IEntity> GetHasRaw<T>(Func<IEnumerable<IHasRawEntity<T>>> source, DataFactoryOptions options) where T: IRawEntity
        {
            var l = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();

            // Get raw entities - from _source or from override method
            var raw = source?.Invoke()?.ToList();

            // If we didn't get anything, return empty
            if (raw.SafeNone())
                return l.Return(DataSourceConstants.EmptyList, "no items returned");

            // Transform result to IEntity
            var result = DataFactory.New(options: options ?? Options).Create(raw);
            return l.Return(result, $"Got {result.Count} items");
        }


    }
}
