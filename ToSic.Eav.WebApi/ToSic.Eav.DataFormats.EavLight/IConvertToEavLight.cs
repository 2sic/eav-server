using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight
{
    /// <summary>
    /// Helper / Service to prepare Entities, Streams and DataSources to <see cref="EavLight"/> for automatic serialization in WebApis. 
    ///
    /// It can prepare single items like <see cref="IEntity"/> and <see cref="IEntityWrapper"/> like DynamicEntities.
    /// It can also prepare IEnumerable/List of these types, as well as **DataStream** and **DataSource** objects. 
    ///
    /// In Custom Code / Razor / WebApi you can get this service with Dependency Injection like <br/> `var converter = GetService&lt;IConvertToJsonLight&gt;()`
    /// </summary>
    [PublicApi]
    public interface IConvertToEavLight: IConvertEntity<EavLightEntity>, IConvertDataSource<EavLightEntity>
    {
    }
}
