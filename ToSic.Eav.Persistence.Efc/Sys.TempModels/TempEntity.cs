using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.Persistence.Efc.Sys.TempModels;

internal class TempEntity
{
    public int EntityId;
    public Guid EntityGuid;
    public int Version;

    public int ContentTypeId;
    public required Target MetadataFor;
    public bool IsPublished;
    public int? PublishedEntityId;
    public required string Owner;
    public DateTime Created;
    public DateTime Modified;
    public required string Json;
    //public string ContentType;
}