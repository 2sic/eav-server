namespace ToSic.Eav.ImportExport.Json.Sys;

/// <summary>
/// WIP
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeWithEntities
{
    public required IContentType ContentType { get; set; }
    public required List<IEntity> Entities { get; set; }
}