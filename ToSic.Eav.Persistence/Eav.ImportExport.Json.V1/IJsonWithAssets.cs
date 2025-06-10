namespace ToSic.Eav.ImportExport.Json.V1;

public interface IJsonWithAssets
{
    ICollection<JsonAsset> Assets { get; set; }
}