namespace ToSic.Eav.ImportExport.Json;

/// <summary>
/// WIP
/// </summary>
public class ContentTypeWithEntities
{
    public IContentType ContentType { get; set; }
    public List<IEntity> Entities { get; set; }
}