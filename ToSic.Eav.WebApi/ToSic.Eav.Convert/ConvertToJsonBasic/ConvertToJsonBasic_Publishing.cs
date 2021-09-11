using System.Collections.Generic;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    public partial class ConvertToJsonLight
    {
        private static void AddPublishingInformation(IEntity entity, IDictionary<string, object> entityValues)
        {
            entityValues.Add(Attributes.RepoIdInternalField, entity.RepositoryId);
            entityValues.Add(Attributes.IsPublishedField, entity.IsPublished);
            if (entity.IsPublished && entity.GetDraft() != null)
            {
                // do a check if there was a field called Published, which we must remove for this to work
                if (entityValues.ContainsKey(Attributes.DraftEntityField))
                    entityValues.Remove(Attributes.DraftEntityField);
                entityValues.Add(Attributes.DraftEntityField, new
                {
                    entity.GetDraft().RepositoryId,
                });
            }

            if (!entity.IsPublished & entity.GetPublished() != null)
            {
                // do a check if there was a field called Published, which we must remove for this to work
                if (entityValues.ContainsKey(Attributes.PublishedEntityField))
                    entityValues.Remove(Attributes.PublishedEntityField);
                entityValues.Add(Attributes.PublishedEntityField, new
                {
                    entity.GetPublished().RepositoryId,
                });
            }
        }

    }
}
