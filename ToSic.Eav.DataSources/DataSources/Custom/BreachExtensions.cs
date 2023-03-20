using System;
using System.Collections.Generic;
using System.Reflection;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Process;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Special - very internal - helper to breach internal APIs in edge cases where they are needed outside.
    /// </summary>
    public static class BreachExtensions
    {
        public static void BreachAutoLoadAllConfigMasks(this DataSource ds, Type type) => ds.AutoLoadAllConfigMasks(type);

        public static CustomDataSourceLight CustomDataSourceLight(CustomDataSourceLight.MyServices services,
            string noParamOrder = Parameters.Protector,
            string logName = null) =>
            new CustomDataSourceLight(services, noParamOrder, logName);

        public static void BreachProvideOut<T>(
            this CustomDataSourceLight ds,
            Func<IEnumerable<IHasRawEntity<T>>> source,
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default) where T : IRawEntity =>
            ds.ProvideOut(source, options: options);

        public static void BreachProvideOut<T>(
            this CustomDataSourceLight ds,
            Func<IEnumerable<T>> source,
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default) where T : IRawEntity =>
            ds.ProvideOut(source, options: options);
    }
}
