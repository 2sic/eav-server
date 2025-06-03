
// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    private static void AddPublishingInformation(IEntity entity, IDictionary<string, object> entityValues, IAppReadEntities appState)
    {
        entityValues.Add(Attributes.RepoIdInternalField, entity.RepositoryId);
        entityValues.Add(Attributes.IsPublishedField, entity.IsPublished);
        if (entity.IsPublished && appState.GetDraft(entity) != null)
        {
            entityValues[Attributes.DraftEntityField] = new
            {
                appState.GetDraft(entity).RepositoryId,
            };
        }

        if (!entity.IsPublished && appState.GetPublished(entity) != null)
        {
            entityValues[Attributes.PublishedEntityField] = new
            {
                appState.GetPublished(entity).RepositoryId,
            };
        }
    }

}