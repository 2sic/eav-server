namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process2PublishAndContentType() : Process0Base("DB.EPrc2")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        var l = services.LogDetails.Fn<EntityProcessData>();

        #region Step 2: check header record - does it already exist, what ID should we use, etc.

        // If we think we'll update an existing entity...
        // ...we have to check if we'll actually update the draft of the entity
        // ...or create a new draft (branch)
        var (existingDraftId, hasAdditionalDraft, entity) = services.PublishingAnalyzer.GetDraftAndCorrectIdAndBranching(data.NewEntity, data.Options, data.LogDetails); // TODO: later just pass in data
        data = data with { NewEntity = entity, }; // may have been replaced with an updated IEntity during corrections

        var (contentTypeId, attributeDefs) = services.StructureAnalyzer.GetContentTypeAndAttribIds(data.SaveJson, data.NewEntity, data.LogDetails);

        data = data with
        {
            IsNew = data.NewEntity.EntityId <= 0, // remember how we want to work...
            ExistingDraftId = existingDraftId,
            HasAdditionalDraft = hasAdditionalDraft,
            ContentTypeId = contentTypeId,
            AttributeDefs = attributeDefs,
        };

        if (data.LogDetails)
            l.A($"entity id:{data.NewEntity.EntityId} - will treat as new:{data.IsNew}");

        #endregion Step 2

        return l.Return(data);
    }

}
