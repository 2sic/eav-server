namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// Mark ContentType Fields which should **not** be included in the content-type definition.
/// </summary>
[WorkInProgressApi("v22")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ContentTypeIgnoreAttribute : Attribute;