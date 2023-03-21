using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Special - very internal - helper to breach internal APIs in edge cases where they are needed outside.
    /// </summary>
    public static class BreachExtensions
    {
        //public static void BreachAutoLoadAllConfigMasks(this DataSource ds, Type type) => ds.AutoLoadAllConfigMasks(type);

        public static CustomDataSourceLight CustomDataSourceLight(CustomDataSourceLight.MyServices services,
            IDataSource wrapper,
            string noParamOrder = Parameters.Protector,
            string logName = null)
        {
            var ds = new CustomDataSourceLight(services, noParamOrder, logName);
            ds.Error.Link(wrapper);
            ds.AutoLoadAllConfigMasks(wrapper.GetType());
            return ds;
        }

        public static void BreachProvideOut(
            this CustomDataSourceLight ds,
            Func<IEnumerable> source,
            string noParamOrder = Parameters.Protector,
            string name = default,
            DataFactoryOptions options = default) =>
            ds.ProvideOut(source, options: options, name: name);

        //public static void BreachProvideOutRaw<T>(
        //    this CustomDataSourceLight ds,
        //    Func<IEnumerable<IHasRawEntity<T>>> source,
        //    string noParamOrder = Parameters.Protector,
        //    DataFactoryOptions options = default) where T : IRawEntity =>
        //    ds.ProvideOutRaw(source, options: options);

        //public static void BreachProvideOutRaw<T>(
        //    this CustomDataSourceLight ds,
        //    Func<IEnumerable<T>> source,
        //    string noParamOrder = Parameters.Protector,
        //    DataFactoryOptions options = default) where T : IRawEntity =>
        //    ds.ProvideOutRaw(source, options: options);

        //public static void BreachProvideOut(
        //    this CustomDataSourceLight ds,
        //    Func<IEnumerable<IEntity>> getList,
        //    string name = DataSourceConstants.StreamDefaultName)
        //    => (ds as DataSource).ProvideOut(getList, name);

        //public static void BreachProvideOut(
        //    this CustomDataSourceLight ds,
        //    Func<IImmutableList<IEntity>> getList,
        //    string name = DataSourceConstants.StreamDefaultName)
        //    => (ds as DataSource).ProvideOut(getList, name);
    }
}
