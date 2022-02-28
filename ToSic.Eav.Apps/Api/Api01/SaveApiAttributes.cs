namespace ToSic.Eav.Apps.Api.Api01
{
    /// <summary>
    /// 
    /// </summary>
    public class SaveApiAttributes
    {
        /// <summary>
        /// Used to set the parent relationship on create item
        /// </summary>
        /// <remarks>New in 13.02</remarks>
        public const string ParentRelationship = "ParentRelationship";
        public const string ParentRelParent = "Parent";
        public const string ParentRelField = "Field";
        public const string ParentRelIndex = "Index";

        // TODO:
        // Should become "PublishState"
        public const string SavePublishingState = "PublishState"; //  Attributes.EntityFieldIsPublished; // "ispublished";

        public const string PublishModeNull = "null";
        public const string PublishModeDraft = "draft";
    }
}
