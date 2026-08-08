namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Mark a field as the title field.
/// </summary>
[WorkInProgressApi("v22")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ContentTypeTitleAttribute : ContentTypeFieldAttribute
{
    public ContentTypeTitleAttribute()
    {
        IsTitle = true;
    }
}