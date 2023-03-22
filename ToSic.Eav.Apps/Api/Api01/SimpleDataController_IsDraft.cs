using ToSic.Eav.Plumbing;
using static ToSic.Eav.Apps.Api.Api01.SaveApiAttributes;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Api.Api01
{
    public partial class SimpleDataController
    {
        public static (bool ShouldPublish, bool ShouldBranchDrafts) GetPublishSpecs(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
        {

            // If it already has a published original
            // Then we want to keep it that way, unless it's not allowed,
            // in which case we must branch (if we will create a draft, to be determined later on)
            var shouldBranchDrafts = existingIsPublished == true && !writePublishAllowed;

            switch (publishedState)
            {
                // Case No change, not specified
                case null:
                case string emptyString when string.IsNullOrEmpty(emptyString):
                case string nullWritten when nullWritten.EqualsInsensitive(PublishModeNull):
                    var published = existingIsPublished != false && writePublishAllowed;
                    return (published, shouldBranchDrafts);

                // Case "draft"
                case string draftString when draftString.EqualsInsensitive(PublishModeDraft):
                    return (false, existingIsPublished ?? false);

                // case boolean or truthy string
                default:
                    var isPublished = publishedState.ConvertOrDefault<bool>(numeric: false, truthy: true);
                    return (isPublished && writePublishAllowed, shouldBranchDrafts);
            }
        }

        //private static bool? IsDraft(Dictionary<string, object> values, IEntity original)
        //{
        //    if (!values.ContainsKey(SaveApiAttributes.SavePublishingState)) return null;
        //    var isPublishedValue = values[SaveApiAttributes.SavePublishingState];
        //    switch (isPublishedValue)
        //    {
        //        // Case No change
        //        case null:
        //        case string emptyString when string.IsNullOrEmpty(emptyString):
        //        case string nullString when nullString.Equals(SaveApiAttributes.PublishModeNull,
        //            StringComparison.InvariantCultureIgnoreCase):
        //            return null;

        //        // Case "draft"
        //        case string draftString when draftString.Equals(SaveApiAttributes.PublishModeDraft,
        //            StringComparison.InvariantCultureIgnoreCase):
        //            // If Original was published then now it should be draft (true)
        //            // Otherwise it should be 
        //            return original?.IsPublished == true;

        //        // case boolean / truthy
        //        default:
        //            var isPublished = isPublishedValue.ConvertOrDefault<bool>(numeric: false, truthy: true);
        //            // TODO: CONVERT TO IF
        //            switch (isPublished)
        //            {
        //                case true: // if IsPublished = true
        //                    return false; // then publish no matter if it was draft or not
        //                case false:
        //                    return true; // make it draft-only - so no original which is still there
        //            }

        //            break;
        //    }

        //    return null;
        //}
    }
}