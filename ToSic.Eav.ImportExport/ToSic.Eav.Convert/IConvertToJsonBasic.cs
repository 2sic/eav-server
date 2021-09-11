using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.ImportExport.Json.Basic;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    /// <summary>
    /// Convert Entities to <see cref="JsonEntity"/>.
    ///
    /// This interface just exists for documentation purposes. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("just fyi")]
    public interface IConvertToJsonBasic: IConvertEntity<JsonEntity>
    {
    }
}
