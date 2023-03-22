using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// A factory to create / initialize data sources.
    ///
    /// Not meant for use in Razor code, but to be used in custom DataSources which may need other internal data sources to work.
    /// </summary>
    /// <remarks>
    /// Released in v15.04
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IDataSourceFactory
    {
        /// <summary>
        /// Get DataSource for specified sourceName/Type.
        /// 
        /// _Note that this is not the preferred way to do things - if possible, use the generic Create below._
        /// </summary>
        /// <param name="type">the .net type of this data-source</param>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="source">optional source to attach as `in` on the newly created data source. It can also provide `AppIdentity` and `LookUp`</param>
        /// <param name="configuration">optional configuration lookup if needed</param>
        /// <returns>A single DataSource</returns>
        /// <remarks>
        /// Released in v15.04
        /// </remarks>
        IDataSource Create(
            Type type,
            string noParamOrder = Parameters.Protector,
            IDataSource source = default, 
            IDataSourceConfiguration configuration = default);

        /// <summary>
        /// Preferred way to create DataSources.
        /// </summary>
        /// <typeparam name="TDataSource">The type of the data source to be created.</typeparam>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="source">optional source to attach as `in` on the newly created data source. It can also provide `AppIdentity` and `LookUp`</param>
        /// <param name="configuration">optional configuration lookup if needed</param>
        /// <returns></returns>
        TDataSource Create<TDataSource>(
            string noParamOrder = Parameters.Protector,
            IDataSource source = default,
            IDataSourceConfiguration configuration = default) where TDataSource : IDataSource;

        /// <summary>
        /// Gets a `Default` DataSource for a specific app. This is a <see cref="PublishingFilter"/> data source which returns the data the current user is allowed to see.
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="configuration">required configuration - must at least have an `AppIdentity`</param>
        /// <returns>A <see cref="PublishingFilter"/> DataSource providing data for this app.</returns>
        IDataSource CreateDefault(
            IDataSourceConfiguration configuration,
            string noParamOrder = Parameters.Protector);
    }
}