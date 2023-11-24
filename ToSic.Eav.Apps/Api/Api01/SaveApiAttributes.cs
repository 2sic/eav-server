namespace ToSic.Eav.Apps.Api.Api01;

/// <summary>
/// 
/// </summary>
public class SaveApiAttributes
{
    /// <summary>
    /// Used to set the parent relationship on create item - so what to link it to after storing
    /// </summary>
    /// <remarks>New in 13.02</remarks>
    public const string ParentRelationship = "ParentRelationship";
    public const string ParentRelParent = "Parent";
    public const string ParentRelField = "Field";
    public const string ParentRelIndex = "Index";

    public const string SavePublishingState = "PublishState";

    public const string PublishModeNull = "null";
    public const string PublishModeDraft = "draft";
}