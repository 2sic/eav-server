
// ReSharper disable once CheckNamespace

using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    private static void AddPublishingInformation(IEntity entity, IDictionary<string, object> entityValues, IAppReadEntities appState)
    {
        entityValues.Add(AttributeNames.RepoIdInternalField, entity.RepositoryId);
        entityValues.Add(AttributeNames.IsPublishedField, entity.IsPublished);
        if (entity.IsPublished && appState.GetDraft(entity) != null)
        {
            entityValues[AttributeNames.DraftEntityField] = new
            {
                appState.GetDraft(entity).RepositoryId,
            };
        }

        if (!entity.IsPublished && appState.GetPublished(entity) != null)
        {
            entityValues[AttributeNames.PublishedEntityField] = new
            {
                appState.GetPublished(entity).RepositoryId,
            };
        }
    }

}