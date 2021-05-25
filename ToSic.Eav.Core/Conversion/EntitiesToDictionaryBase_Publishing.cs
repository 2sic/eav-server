﻿using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Conversion
{
    public abstract partial class EntitiesToDictionaryBase
    {
        private static void AddPublishingInformation(IEntity entity, Dictionary<string, object> entityValues)
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
