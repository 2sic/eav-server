namespace ToSic.Eav.Ui.Context.Sys.Dto;

public class InstanceDto
{
    /// <summary>
    /// The Module Id / Instance Id.
    /// Note that an instance can be used again, in which case the UsageId will differ
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The usage id - in case an instance is reused.
    /// </summary>
    public int UsageId { get; init; }

    public bool ShowOnAllPages { get; init; }
    public string? Title { get; init; }
    public bool IsDeleted { get; init; }
    public PageDto? Page { get; init; }
}