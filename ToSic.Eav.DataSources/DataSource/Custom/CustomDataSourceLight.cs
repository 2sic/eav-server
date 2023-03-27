using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSources.DataSourceConstants;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Very lightweight DataSource base for data sources which are very simple and convention based.
    /// </summary>
    public class CustomDataSourceLight: CustomDataSourceAdvanced
    {

        protected internal CustomDataSourceLight(MyServices services, string logName = null) : base(services, logName ?? "Ds.CustLt")
        {
            // Provide a default out, in case the overriding class doesn't
            base.ProvideOut(() => GetRaw(GetDefault, null));
        }

        protected virtual DataFactoryOptions Options
        {
            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            get => _options ?? (_options = new DataFactoryOptions(typeName: "Custom"));
            set => _options = value;
        }
        private DataFactoryOptions _options;

        protected virtual IEnumerable<IRawEntity> GetDefault() => new List<IRawEntity>();

        /// <summary>
        /// Provide data on the `Out` of this DataSource.
        /// This is a very generic version which takes any function that generates a list of something.
        /// Internally it will try to detect what the data was and convert it to the final format.
        ///
        /// Note that the `source` must create a list (`IEnumerable`) of any of the following (all items must have the same type):
        /// * <see cref="IEntity"/>
        /// * <see cref="IRawEntity"/>
        /// * <see cref="IHasRawEntity{T}"/>
        ///
        /// If you know what data type you're creating, you should look at the other ProvideOut* methods.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="name">_optional_ name of the out-stream.</param>
        /// <param name="options">Conversion options which are relevant for <see cref="IRawEntity"/> data</param>
        protected internal void ProvideOut(
            Func<IEnumerable> data,
            string noParamOrder = Parameters.Protector,
            string name = StreamDefaultName,
            Func<DataFactoryOptions> options = default)
        {
            Parameters.Protect(noParamOrder, $"{nameof(name)}, {nameof(options)}");
            base.ProvideOut(() => GetAny(data, options), name);
        }


        protected internal void ProvideOutRaw<T>(
            Func<IEnumerable<IHasRawEntity<T>>> data,
            string noParamOrder = Parameters.Protector,
            string name = StreamDefaultName,
            Func<DataFactoryOptions> options = default) where T : IRawEntity
        {
            Parameters.Protect(noParamOrder, $"{nameof(name)}, {nameof(options)}");
            base.ProvideOut(() => GetHasRaw(data, options), name);
        }

        protected internal void ProvideOutRaw<T>(
            Func<IEnumerable<T>> data,
            string noParamOrder = Parameters.Protector,
            string name = StreamDefaultName,
            Func<DataFactoryOptions> options = default) where T : IRawEntity
        {
            Parameters.Protect(noParamOrder, $"{nameof(name)}, {nameof(options)}");
            base.ProvideOut(() => GetRaw(data, options), name);
        }

        private IImmutableList<IEntity> GetAny(Func<IEnumerable> source, Func<DataFactoryOptions> options)
        {
            var l = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();
            
            List<object> data;
            try
            {
                data = source?.Invoke().Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                l.Ex(ex);
                var runErr = Error.Create(title: $"Error calling source generator of {nameof(ProvideOut)}. " +
                                                 "Error details can be found in Insights.");
                return l.Return(runErr, "error");
            }

            if (data.SafeNone()) l.Return(EmptyList, "no items returned");

            if (data.All(i => i is IEntity))
                return l.Return(data.Cast<IEntity>().ToImmutableList(), "IEntities");

            if (data.All(i => i is IRawEntity))
            {
                var raw = data.Cast<IRawEntity>().ToList();
                var result = DataFactory.New(options: GetBest(options)).Create(raw);
                return l.Return(result, "was IRawEntity");
            }

            // Do this first, to make all the data be IRawEntity
            if (data.All(i => i is IHasRawEntity))
            {
                var raw = data.Cast<IHasRawEntity<IRawEntity>>().ToList();
                var result = DataFactory.New(options: GetBest(options)).Create(raw);
                return l.Return(result, "was IHasRawEntity");
            }

            // todo - maybe also process IHasEntity - but only after doing the raw entities

            var err = Error.Create(title: $"Error in {nameof(ProvideOutRaw)}",
                message: "The list received was tested against all possible data types but non matched. " +
                         $"Expected was a list of either {nameof(IEntity)}, {nameof(IRawEntity)}, {nameof(IHasRawEntity<IRawEntity>)}. " +
                         "Note that all items must be of the same type. ");
            return l.Return(err, "error");
        }

        private DataFactoryOptions GetBest(Func<DataFactoryOptions> options) => options?.Invoke() ?? Options;

        private IImmutableList<IEntity> GetRaw<T>(Func<IEnumerable<T>> source, Func<DataFactoryOptions> options) where T: IRawEntity
        {
            var l = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();

            // Get raw entities - from _source or from override method
            var raw = source?.Invoke()?.ToList();

            // If we didn't get anything, return empty
            if (raw.SafeNone())
                return l.Return(EmptyList, "no items returned");

            // Transform result to IEntity
            var result = DataFactory.New(options: GetBest(options)).Create(raw);
            return l.Return(result, $"Got {result.Count} items");
        }

        private IImmutableList<IEntity> GetHasRaw<T>(Func<IEnumerable<IHasRawEntity<T>>> source, Func<DataFactoryOptions> options) where T: IRawEntity
        {
            var l = Log.Fn<IImmutableList<IEntity>>();
            Configuration.Parse();

            // Get raw entities - from _source or from override method
            var raw = source?.Invoke()?.ToList();

            // If we didn't get anything, return empty
            if (raw.SafeNone())
                return l.Return(EmptyList, "no items returned");

            // Transform result to IEntity
            var result = DataFactory.New(options: GetBest(options)).Create(raw);
            return l.Return(result, $"Got {result.Count} items");
        }


    }
}
