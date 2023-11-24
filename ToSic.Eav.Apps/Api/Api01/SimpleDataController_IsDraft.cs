using ToSic.Eav.Plumbing;
using static ToSic.Eav.Apps.Api.Api01.SaveApiAttributes;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Api.Api01;

partial class SimpleDataController
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
    
}