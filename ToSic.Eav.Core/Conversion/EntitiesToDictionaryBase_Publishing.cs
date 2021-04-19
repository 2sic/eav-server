using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Conversion
{
    public abstract partial class EntitiesToDictionaryBase
    {
        private static void AddPublishingInformation(IEntity entity, Dictionary<string, object> entityValues)
        {
            entityValues.Add(Constants.RepoIdInternalField, entity.RepositoryId);
            entityValues.Add(Constants.IsPublishedField, entity.IsPublished);
            if (entity.IsPublished && entity.GetDraft() != null)
            {
                // do a check if there was a field called Published, which we must remove for this to work
                if (entityValues.ContainsKey(Constants.DraftEntityField))
                    entityValues.Remove(Constants.DraftEntityField);
                entityValues.Add(Constants.DraftEntityField, new
                {
                    entity.GetDraft().RepositoryId,
                });
            }

            if (!entity.IsPublished & entity.GetPublished() != null)
            {
                // do a check if there was a field called Published, which we must remove for this to work
                if (entityValues.ContainsKey(Constants.PublishedEntityField))
                    entityValues.Remove(Constants.PublishedEntityField);
                entityValues.Add(Constants.PublishedEntityField, new
                {
                    entity.GetPublished().RepositoryId,
                });
            }
        }

    }
}
