﻿using ToSic.Eav.Persistence.Sys.Logging;

namespace ToSic.Eav.Repository.Efc.Sys.DbContentTypes;

partial class DbContentType
{

    /// <summary>
    /// if AttributeSet refers another Content-Type, get ID of the referred Content-Type. Otherwise, returns passed ContentTypeId.
    /// </summary>
    /// <param name="contentTypeId">ContentTypeId to resolve</param>
    internal int ResolvePotentialGhostContentTypeId(int contentTypeId)
    {
        var usesConfigurationOfContentType = DbStore.SqlDb.TsDynDataContentTypes
            .Where(a => a.ContentTypeId == contentTypeId)
            .Select(a => a.InheritContentTypeId)
            .Single();
        return usesConfigurationOfContentType ?? contentTypeId;
    }

    private List<TsDynDataContentType> FindPotentialGhostSources(string contentTypeParentName)
    {
        var ghostType = DbStore.SqlDb.TsDynDataContentTypes.Where(
                a => a.StaticName == contentTypeParentName
                     && a.TransDeletedId == null
                     && a.InheritContentTypeId == null).
            OrderBy(a => a.ContentTypeId)
            .ToList();
        return ghostType;
    }


    public void CreateGhost(string staticName)
    {
        var ct = TryGetTypeByStaticTracked(staticName);
        if (ct != null)
            throw new("current App already has a content-type with this static name - cannot continue");

        // find the original
        var attSets = DbStore.SqlDb.TsDynDataContentTypes
            .Where(ats => ats.StaticName == staticName
                          && !ats.InheritContentTypeId.HasValue    // never duplicate a clone/ghost
                          && ats.TransDeletedId == null                 // never duplicate a deleted
                          && ats.IsGlobal == false)           // never duplicate an always-share
            .OrderBy(ats => ats.ContentTypeId)
            .ToList();

        if (!attSets.Any())
            throw new ArgumentException("can't find an original, non-ghost content-type with the static name '" + staticName + "'");

        if (attSets.Count > 1)
            throw new("found " + attSets.Count + " (expected 1) original, non-ghost content-type with the static name '" + staticName + "' - so won't create ghost as it's not clear off which you would want to ghost.");

        var attSet = attSets.First();
        var newSet = new TsDynDataContentType
        {
            AppId = DbStore.AppId, // needs the new, current appid
            StaticName = attSet.StaticName,
            Name = attSet.Name,
            Scope = attSet.Scope,
            InheritContentTypeId = attSet.ContentTypeId,
            IsGlobal = false, // this is copy, never re-share
            TransCreatedId = DbStore.Versioning.GetTransactionId()
        };

        // save first, to ensure it has an Id
        DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.Add(newSet));
    }


    /// <summary>
    /// Look up the ghost-parent-id
    /// </summary>
    /// <returns>The parent id as needed, or 0 if not found - which usually indicates an import problem</returns>
    private int FindGhostParentIdOrLogWarnings(string contentTypeParentName)
    {
        // Look for the potential source of this ghost
        var ghostContentTypes = DbStore.ContentType.FindPotentialGhostSources(contentTypeParentName);

        if (ghostContentTypes.Count == 1)
            return ghostContentTypes.First().ContentTypeId;

        // If multiple masters are found, use first and add a warning message
        if (ghostContentTypes.Count > 1)
            DbStore.ImportLogToBeRefactored.Add(new($"Multiple potential master Content-Types found for StaticName: {contentTypeParentName}", Message.MessageTypes.Warning));

        // nothing found - report error
        DbStore.ImportLogToBeRefactored.Add(new($"Content-Type not imported because master set not found: {contentTypeParentName}", Message.MessageTypes.Warning));
        return 0;
    }

}