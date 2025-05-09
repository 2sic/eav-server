namespace ToSic.Eav.Persistence.Efc.Intermediate;

internal class TempEntity
{
    public int EntityId;
    public Guid EntityGuid;
    public int Version;

    public int ContentTypeId;
    public Target MetadataFor;
    public bool IsPublished;
    public int? PublishedEntityId;
    public string Owner;
    public DateTime Created;
    public DateTime Modified;
    public string Json;
    //public string ContentType;
}