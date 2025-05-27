namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbContentType
{

    /// <summary>
    /// if AttributeSet refers another AttributeSet, get ID of the referred AttributeSet. Otherwise returns passed ContentTypeId.
    /// </summary>
    /// <param name="contentTypeId">ContentTypeId to resolve</param>
    internal int ResolvePotentialGhostContentTypeId(int contentTypeId)
    {
        var usesConfigurationOfContentType = DbContext.SqlDb.TsDynDataContentTypes
            .Where(a => a.ContentTypeId == contentTypeId)
            .Select(a => a.InheritContentTypeId)
            .Single();
        return usesConfigurationOfContentType ?? contentTypeId;
    }

    private List<TsDynDataContentType> FindPotentialGhostSources(string contentTypeParentName)
    {
        var ghostAttributeSets = DbContext.SqlDb.TsDynDataContentTypes.Where(
                a => a.StaticName == contentTypeParentName
                     && a.TransDeletedId == null
                     && a.InheritContentTypeId == null).
            OrderBy(a => a.ContentTypeId)
            .ToList();
        return ghostAttributeSets;
    }


    public void CreateGhost(string staticName)
    {
        var ct = GetTypeByStaticName(staticName);
        if (ct != null)
            throw new("current App already has a content-type with this static name - cannot continue");

        // find the original
        var attSets = DbContext.SqlDb.TsDynDataContentTypes
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
            AppId = DbContext.AppId, // needs the new, current appid
            StaticName = attSet.StaticName,
            Name = attSet.Name,
            Scope = attSet.Scope,
            InheritContentTypeId = attSet.ContentTypeId,
            IsGlobal = false, // this is copy, never re-share
            TransCreatedId = DbContext.Versioning.GetTransactionId()
        };
        DbContext.SqlDb.Add(newSet);

        // save first, to ensure it has an Id
        DbContext.SqlDb.SaveChanges();
    }


    /// <summary>
    /// Look up the ghost-parent-id
    /// </summary>
    /// <returns>The parent id as needed, or 0 if not found - which usually indicates an import problem</returns>
    private int FindGhostParentIdOrLogWarnings(string contentTypeParentName)
    {
        // Look for the potential source of this ghost
        var ghostAttributeSets = DbContext.ContentType.FindPotentialGhostSources(contentTypeParentName);

        if (ghostAttributeSets.Count == 1)
            return ghostAttributeSets.First().ContentTypeId;

        // If multiple masters are found, use first and add a warning message
        if (ghostAttributeSets.Count > 1)
            DbContext.ImportLogToBeRefactored.Add(new($"Multiple potential master AttributeSets found for StaticName: {contentTypeParentName}", Message.MessageTypes.Warning));

        // nothing found - report error
        DbContext.ImportLogToBeRefactored.Add(new($"AttributeSet not imported because master set not found: {contentTypeParentName}", Message.MessageTypes.Warning));
        return 0;
    }

}