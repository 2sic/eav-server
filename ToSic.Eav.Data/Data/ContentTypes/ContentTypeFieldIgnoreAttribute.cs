namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Mark ContentType Properties which should not be converted into Raw Entities
/// </summary>
[PrivateApi("WIP")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ContentTypeFieldIgnoreAttribute : Attribute;