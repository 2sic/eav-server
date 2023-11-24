using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

// ReSharper disable InheritdocInvalidUsage
#pragma warning disable CS0108, CS0114

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// Helper / Service to prepare Entities, Streams and DataSources to <see cref="EavLight"/> for automatic serialization in WebApis. 
///
/// It can prepare single items like <see cref="IEntity"/> and <see cref="IEntityWrapper"/> like DynamicEntities.
/// It can also prepare IEnumerable/List of these types, as well as **DataStream** and **DataSource** objects. 
///
/// In Custom Code / Razor / WebApi you can get this service with Dependency Injection like <br/>
/// `
/// var converter = GetService&lt;IConvertToEavLight&gt;();
/// `
/// </summary>
/// <remarks>
/// We're standardizing how conversions are done using the <see cref="IConvert{TFrom,TTo}"/> interface.
/// This is why you don't see any methods on this interface in the docs.
/// In most cases you'll just do `var prepared = converter.Convert(someEntityObjectOrList);`
/// </remarks>
[PublicApi]
public interface IConvertToEavLight: IConvertEntity<EavLightEntity>, IConvertDataSource<EavLightEntity>, IHasLog
{
    /// <inheritdoc />
    IEnumerable<EavLightEntity> Convert(IEnumerable<IEntity> entities);

    /// <inheritdoc />
    EavLightEntity Convert(IEntity entity);

    /// <inheritdoc />
    IEnumerable<EavLightEntity> Convert(IEnumerable<object> list);

    /// <inheritdoc />
    EavLightEntity Convert(object item);


    /// <inheritdoc />
    IEnumerable<EavLightEntity> Convert(IEnumerable<IEntityWrapper> list);

    /// <inheritdoc />
    EavLightEntity Convert(IEntityWrapper item);

    /// <inheritdoc />
    IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, IEnumerable<string> streams = null);

    /// <inheritdoc />
    IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, IEnumerable<string> streams, string[] filterGuids);

    /// <inheritdoc />
    IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, string streams);
}