namespace ToSic.Eav.WebApi.Sys.Cms;

/// <summary>
/// Important: Empty constructor is needed for JSON serialization.
/// </summary>
public record EntityInListDto()
{
    public int Index { get; init; }
    public int Id { get; init; }
    public Guid Guid { get; init; }
    public string? Title { get; init; }
    public string? Type { get; init; }

    /// <summary>
    /// Quick constructor to create from an entity.
    /// </summary>
    public EntityInListDto(IEntity? entity, int index): this()
    {
        Index = index;
        Id = entity?.EntityId ?? 0;
        Guid = entity?.EntityGuid ?? Guid.Empty;
        Title = entity?.GetBestTitle() ?? "";
        Type = entity?.Type.NameId;
    }
}