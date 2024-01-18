namespace ToSic.Eav.Data;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Added in 13.02
/// IMPORTANT: Don't cache this object, as some info inside it can change during runtime
/// </remarks>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentTypeDetails(IEntity entity) : EntityBasedType(entity)
{
    public const string ContentTypeTypeName = "ContentType";

    public string Icon => GetThis<string>(null);

    public string DynamicChildrenField => GetThis<string>(null);

    public string Description => GetThis<string>(null);

    public string AdditionalSettings => GetThis<string>(null);
}