using ToSic.Eav.DataSource;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Services;

/// <summary>
/// A factory/generator to create one specific kind of data source.
///
/// _Note: This is not meant for use in Razor code, but to be used in custom DataSources which may need other internal data sources to work._
///
/// Where possible, use the Generator instead of the <see cref="IDataSourcesService"/>.
/// The Generator makes it clearer when you only need to use a single typed DataSource and not need access to all kinds of DataSources.
/// </summary>
/// <remarks>
/// Released in v15.06
/// </remarks>
/// <typeparam name="T">The type of the data source to be created.</typeparam>
[PublicApi]
public interface IDataSourceGenerator<out T> where T : IDataSource
{
    /// <summary>
    /// Preferred way to create DataSources.
    /// </summary>
    /// <param name="attach">optional source to attach as `in` on the newly created data source. If provided, it can also provide `appIdentity` and `configSource`</param>
    /// <param name="options">optional configuration</param>
    /// <returns></returns>
    T New(IDataSourceLinkable attach = default, IDataSourceOptions options = default);
}