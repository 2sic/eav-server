namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// WIP
/// Use this to specify raw data which - when converted to an entity - should be assigned to a specific content-type.
/// </summary>
[WorkInProgressApi("v22")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[AttributeUsage(AttributeTargets.Class)]
public class ContentTypeAssignAttribute : Attribute
{
    public required Type Type { get; init; }
}
