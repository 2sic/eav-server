﻿using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight
{
    public partial class ConvertToEavLight
    {
        private static void AddPublishingInformation(IEntity entity, IDictionary<string, object> entityValues, AppState appState)
        {
            entityValues.Add(Attributes.RepoIdInternalField, entity.RepositoryId);
            entityValues.Add(Attributes.IsPublishedField, entity.IsPublished);
            if (entity.IsPublished && appState.GetDraft(entity) != null)
            {
                // do a check if there was a field called Published, which we must remove for this to work
                if (entityValues.ContainsKey(Attributes.DraftEntityField))
                    entityValues.Remove(Attributes.DraftEntityField);
                entityValues.Add(Attributes.DraftEntityField, new
                {
                    appState.GetDraft(entity).RepositoryId,
                });
            }

            if (!entity.IsPublished & appState.GetPublished(entity) != null)
            {
                // do a check if there was a field called Published, which we must remove for this to work
                if (entityValues.ContainsKey(Attributes.PublishedEntityField))
                    entityValues.Remove(Attributes.PublishedEntityField);
                entityValues.Add(Attributes.PublishedEntityField, new
                {
                    appState.GetPublished(entity).RepositoryId,
                });
            }
        }

    }
}
