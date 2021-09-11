using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.JsonLight
{
    /// <summary>
    /// Service to Convert Entities, Streams and DataSources to <see cref="JsonLight"/> for automatic serialization in WebApis. 
    ///
    /// It can convert single items like <see cref="IEntity"/> and <see cref="IEntityWrapper"/> like DynamicEntities.
    ///
    /// In Custom Code / Razor / WebApi you can get this service with `var converter = GetService&lt;IConvertToJsonLight&gt;()`
    /// </summary>
    [PublicApi]
    public interface IConvertToJsonLight: IConvertEntity<JsonEntity>, IConvertDataSource<JsonEntity>
    {
    }
}
