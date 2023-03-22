using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.LookUp;
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
    [PublicApi]
    public interface IDataSourceFactory
    {
        /// <summary>
        /// Get DataSource for specified sourceName/Type.
        /// 
        /// _Note that this is not the preferred way to do things - if possible, use the generic Create below._
        /// </summary>
        /// <param name="type">the .net type of this data-source</param>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="source">optional source to attach as `in` on the newly created data source. If provided, it can also provide `appIdentity` and `configSource`</param>
        /// <param name="appIdentity">optional identity, usually necessary if `source` was not specified</param>
        /// <param name="configuration">optional configuration lookup if needed</param>
        /// <returns>A single DataSource</returns>
        /// <remarks>
        /// Released in v15.04
        /// </remarks>
        IDataSource Create(
            Type type,
            string noParamOrder = Parameters.Protector,
            IDataSource source = default, 
            IAppIdentity appIdentity = default,
            IConfiguration configuration = default);

        /// <summary>
        /// Preferred way to create DataSources.
        /// </summary>
        /// <typeparam name="TDataSource">The type of the data source to be created.</typeparam>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="source">optional source to attach as `in` on the newly created data source. If provided, it can also provide `appIdentity` and `configSource`</param>
        /// <param name="appIdentity">optional identity, usually necessary if not `source` was specified</param>
        /// <param name="configuration">optional configuration lookup if needed</param>
        /// <returns></returns>
        TDataSource Create<TDataSource>(
            string noParamOrder = Parameters.Protector,
            IDataSource source = default,
            IAppIdentity appIdentity = default,
            IConfiguration configuration = default) where TDataSource : IDataSource;

        /// <summary>
        /// Gets a `Default` DataSource for a specific app. This is a <see cref="PublishingFilter"/> data source which returns the data the current user is allowed to see.
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="appIdentity">identity, **required**</param>
        /// <param name="showDrafts">optional override whether Draft Entities should be returned</param>
        /// <param name="configuration">optional configuration lookup if needed</param>
        /// <returns>A <see cref="PublishingFilter"/> DataSource providing data for this app.</returns>
        IDataSource CreateDefault(
            IAppIdentity appIdentity,
            string noParamOrder = Parameters.Protector,
            bool? showDrafts = default, 
            IConfiguration configuration = default);
    }
}