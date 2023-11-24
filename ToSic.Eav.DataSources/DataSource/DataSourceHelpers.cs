using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSource;

internal static class DataSourceHelpers
{
    public static T Init<T>(this T thisDs, ILookUpEngine lookUpEngine) where T : IDataSource
    {
        if (lookUpEngine != null && thisDs.Configuration is DataSourceConfiguration dsConfig)
            dsConfig.LookUpEngine = lookUpEngine;
        return thisDs;
    }

    ///// <summary>
    ///// Helper function (internal) to configure a new data source.
    ///// </summary>
    ///// <param name="newDataSource">The new data source</param>
    ///// <param name="attach"></param>
    ///// <param name="options">optional configuration provider</param>
    //public static T Init<T>(this T newDataSource, IDataSourceLinkable attach, IDataSourceOptions options) where T : IDataSource
    //{
    //    if (newDataSource == null) throw new ArgumentNullException(nameof(newDataSource));

    //    // Set immutable and more
    //    newDataSource.Setup(options, attach);

    //    var mainUpstream = attach?.Link?.DataSource;
    //    (newDataSource as IAppIdentitySync)?.UpdateAppIdentity(options?.AppIdentity ?? mainUpstream);

    //    //if (source != null) newDataSource.Attach(source);
    //    if (attach?.Link != null) newDataSource.Connect(attach.Link);

    //    var lookUp = options?.LookUp ?? mainUpstream?.Configuration?.LookUpEngine;
    //    if (lookUp != null && newDataSource.Configuration is DataSourceConfiguration dsConfig)
    //    {
    //        dsConfig.LookUpEngine = lookUp;
    //        var configValues = options?.Values;
    //        if (configValues != null) dsConfig.AddMany(configValues.ToEditable());
    //    }
    //    return newDataSource;
    //}
}