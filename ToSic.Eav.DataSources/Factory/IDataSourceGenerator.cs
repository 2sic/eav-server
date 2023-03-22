using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.LookUp;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// A factory/generator to create one specific kind of data source.
    ///
    /// _Note: This is not meant for use in Razor code, but to be used in custom DataSources which may need other internal data sources to work._
    ///
    /// Where possible, use the Generator instead of the <see cref="IDataSourceFactory"/>.
    /// The Generator makes it clearer when you only need to use a single typed DataSource and not need access to all kinds of DataSources.
    /// </summary>
    /// <remarks>
    /// Released in v15.04
    /// </remarks>
    /// <typeparam name="TDataSource">The type of the data source to be created.</typeparam>
    [PublicApi]
    public interface IDataSourceGenerator<out TDataSource> where TDataSource : IDataSource
    {
        /// <summary>
        /// Preferred way to create DataSources.
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="source">optional source to attach as `in` on the newly created data source. If provided, it can also provide `appIdentity` and `configSource`</param>
        /// <param name="configuration">optional configuration</param>
        /// <returns></returns>
        TDataSource New(
            string noParamOrder = Parameters.Protector,
            IDataSource source = default,
            IDataSourceConfiguration configuration = default);
    }
}