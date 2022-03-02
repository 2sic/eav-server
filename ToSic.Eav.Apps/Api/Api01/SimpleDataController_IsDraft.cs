using System;
using ToSic.Eav.Apps.Api.Api01;
using ToSic.Eav.Plumbing;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Api.Api01
{
    public partial class SimpleDataController
    {
        public static (bool published, bool branch) IsDraft(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
        {
            bool published, branch;

            if (!existingIsPublished.HasValue) // when creating new
                branch = false; // false because is it new one, so no branch
            else // update published, draft, published and draft
                branch = existingIsPublished.Value && !writePublishAllowed;


            switch (publishedState)
            {
                // Case No change
                case null:
                case string emptyString when string.IsNullOrEmpty(emptyString):
                case string nullString when nullString.Equals(SaveApiAttributes.PublishModeNull,
                    StringComparison.InvariantCultureIgnoreCase):
                    published = !existingIsPublished.HasValue
                        ? writePublishAllowed
                        : existingIsPublished.Value && writePublishAllowed;
                    break;

                // Case "draft"
                case string draftString when draftString.Equals(SaveApiAttributes.PublishModeDraft,
                    StringComparison.InvariantCultureIgnoreCase):
                    published = false;
                    branch = existingIsPublished ?? true;
                    break;

                // case boolean / truthy
                default:
                    var savePublished = publishedState.ConvertOrDefault<bool>(numeric: false, truthy: true);
                    published = savePublished && writePublishAllowed;
                    break;
            }

            // TODO: STV find is it better to return just NULL (for value tuple) when branch == false

            return (published, branch);
        }

        public static (bool published, bool branch) DraftAndBranch(bool draft) => (!draft, draft);


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