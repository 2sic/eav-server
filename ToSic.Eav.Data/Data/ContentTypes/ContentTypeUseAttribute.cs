namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Mark raw data objects which will be converted to Entities,
/// so that they will inherit the content-type definition of another type.
/// </summary>
/// <remarks>
/// Use this to specify raw data which - when converted to an entity - should be assigned to a specific content-type.
/// 
/// This is especially important when the **Content Type Definition** is on another interface or class,
/// but your **Raw Data Object** must reference that.
/// </remarks>
[WorkInProgressApi("v22")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[AttributeUsage(AttributeTargets.Class)]
public class ContentTypeUseAttribute : Attribute
{
    public required Type Type { get; init; }
}
