using System;
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
            Provide(GetLight);
        }

        protected virtual DataFactorySettings Options
        {
            get => _options ?? (_options = new DataFactorySettings(typeName: "Custom"));
            set => _options = value;
        }
        private DataFactorySettings _options;

        protected void ProvideOut(
            IEnumerable<IRawEntity> source,
            string noParamOrder = Parameters.Protector,
            DataFactorySettings options = default) =>
            ProvideOut(() => source, options: options);

        protected void ProvideOut(
            Func<IEnumerable<IRawEntity>> source,
            string noParamOrder = Parameters.Protector,
            DataFactorySettings options = default)
        {
            _source = source;
            if (options != null) Options = options;
            Provide(GetLight);
        }
        private Func<IEnumerable<IRawEntity>> _source;

        private IImmutableList<IEntity> GetLight()
        {
            var l = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();

            // Get raw entities - from _source or from override method
            List<IRawEntity> raw;
            if (_source != null)
            {
                l.A($"Has a {nameof(_source)} generator, will use this.");
                raw = _source.Invoke()?.ToList();
            }
            else
            {
                l.A($"No source function found, will use {nameof(GetDefault)}");
                raw = GetDefault()?.ToList();
            }

            // If we didn't get anything, return empty
            if (raw.SafeNone())
                return l.Return(DataSourceConstants.EmptyList, "no items returned");

            // Transform result to IEntity
            var result = DataFactory.New(options: Options).Create(raw);
            return l.Return(result, $"Got {result.Count} items");
        }

        protected virtual IEnumerable<IRawEntity> GetDefault() => new List<IRawEntity>();
    }
}
