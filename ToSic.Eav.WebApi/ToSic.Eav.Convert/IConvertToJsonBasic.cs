using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using ToSic.Eav.ImportExport.Json.Basic;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    /// <summary>
    /// Service to Convert Entities, Streams and DataSources to JsonBasic <see cref="JsonEntity"/> for automatic serialization in WebApis. 
    ///
    /// It can convert single items like <see cref="IEntity"/> and <see cref="IEntityWrapper"/> like DynamicEntities.
    ///
    /// In Custom Code / Razor / WebApi you can get this service with `var converter = GetService&lt;IConvertToDictionary&gt;()`
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("just fyi")]
    public interface IConvertToJsonBasic: IConvertEntity<JsonEntity>, IConvertDataSource<JsonEntity>
    {
    }
}
