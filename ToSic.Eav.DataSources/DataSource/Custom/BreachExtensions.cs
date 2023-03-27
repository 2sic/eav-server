using System;
using System.Collections;
using ToSic.Eav.Data.Build;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSource
{
    /// <summary>
    /// Special - very internal - helper to breach internal APIs in edge cases where they are needed outside.
    /// </summary>
    public static class BreachExtensions
    {
        public static CustomDataSourceLight CustomDataSourceLight(CustomDataSourceLight.MyServices services,
            IDataSource wrapper,
            string noParamOrder = Parameters.Protector,
            string logName = null)
        {
            var ds = new CustomDataSourceLight(services, logName);
            ds.Error.Link(wrapper);
            ds.AutoLoadAllConfigMasks(wrapper.GetType());
            return ds;
        }

        public static void BreachProvideOut(
            this CustomDataSourceLight ds,
            Func<IEnumerable> source,
            string noParamOrder = Parameters.Protector,
            string name = StreamDefaultName,
            Func<DataFactoryOptions> options = default) =>
            ds.ProvideOut(source, options: options, name: name);
    }
}
