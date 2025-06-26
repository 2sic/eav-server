using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    private static void AddPublishingInformation(IEntity entity, IDictionary<string, object?> entityValues, IAppReadEntities appState)
    {
        entityValues.Add(AttributeNames.RepoIdInternalField, entity.RepositoryId);
        entityValues.Add(AttributeNames.IsPublishedField, entity.IsPublished);
        if (entity.IsPublished && appState.GetDraft(entity) is { } draft)
            entityValues[AttributeNames.DraftEntityField] = new { draft.RepositoryId, };

        if (!entity.IsPublished && appState.GetPublished(entity) is { } published)
            entityValues[AttributeNames.PublishedEntityField] = new { published.RepositoryId, };
    }

}