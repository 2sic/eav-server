namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Mark ContentType Fields which should not be converted into Raw Entities
/// </summary>
[WorkInProgressApi("v22")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ContentTypeIgnoreAttribute : Attribute;