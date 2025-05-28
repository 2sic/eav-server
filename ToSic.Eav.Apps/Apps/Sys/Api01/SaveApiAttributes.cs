namespace ToSic.Eav.Apps.Internal.Api01;

/// <summary>
/// Special keys / values used in the dictionary to save, based on which the system will decide how to handle drafts/publishing
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
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