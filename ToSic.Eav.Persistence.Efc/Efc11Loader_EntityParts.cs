using ToSic.Eav.Persistence.Efc.Intermediate;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc;

partial class Efc11Loader
{
    private int[] GetEntityIdOfPartnerEntities(int[] repositoryIds)
    {
        var l = Log.Fn<int[]>();
        var relatedIds = from e in _dbContext.ToSicEavEntities
            where e.PublishedEntityId.HasValue && !e.IsPublished && repositoryIds.Contains(e.EntityId) &&
                  !repositoryIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
            select e.PublishedEntityId.Value;

        var combined = repositoryIds.Union(relatedIds).ToArray();

        return l.ReturnAsOk(combined);
    }


    private List<TempEntity> GetRawEntities(int[] entityIds, int appId, bool filterIds, string filterType = null)
    {
        var l = Log.Fn<List<TempEntity>>($"app: {appId}, ids: {entityIds.Length}, filter: {filterIds}; {nameof(filterType)}: '{filterType}'");
        var query = _dbContext.ToSicEavEntities
            .Include(e => e.AttributeSet)
            .Where(e => e.AppId == appId)
            .Where(e => e.ChangeLogDeleted == null && e.AttributeSet.ChangeLogDeleted == null);

        // filter by EntityIds (if set)
        if (filterIds)
            query = query.Where(e => entityIds.Contains(e.EntityId));

        if (filterType != null)
            query = query.Where(e => e.ContentType == filterType);

        var rawEntities = query
            .OrderBy(e => e.EntityId) // order to ensure drafts are processed after draft-parents
            .Select(e => new TempEntity
            {
                EntityId = e.EntityId,
                EntityGuid = e.EntityGuid,
                Version = e.Version,
                AttributeSetId = e.AttributeSetId,
                MetadataFor = new(e.AssignmentObjectTypeId, null, e.KeyString, e.KeyNumber, e.KeyGuid),
                IsPublished = e.IsPublished,
                PublishedEntityId = e.PublishedEntityId,
                Owner = e.Owner,
                Created = e.ChangeLogCreatedNavigation.Timestamp,
                Modified = e.ChangeLogModifiedNavigation.Timestamp,
                Json = e.Json,
            })
            .ToList();

        return l.Return(rawEntities, $"found: {rawEntities.Count}");
    }

    private Dictionary<int, IEnumerable<TempAttributeWithValues>> GetAttributesOfEntityChunk(List<int> entityIdsFound)
    {
        var l = Log.Fn<Dictionary<int, IEnumerable<TempAttributeWithValues>>>(
            $"ids: {entityIdsFound.Count}");
            
        // just get once, we'll need it in a deep loop
        var primaryLanguage = PrimaryLanguage;

        var attributes = _dbContext.ToSicEavValues
            .Include(v => v.Attribute)
            .Include(v => v.ToSicEavValuesDimensions)
            .ThenInclude(d => d.Dimension)
            .Where(r => entityIdsFound.Contains(r.EntityId))
            .Where(v => !v.ChangeLogDeleted.HasValue)
            // ToList is necessary because groupby actually runs on dotnet (not SQL).
            // Efcore 1 did this implicitly, efcore 3.x need to do it explicitly.
            .ToList()
            .GroupBy(e => e.EntityId)
            .ToDictionary(e => e.Key, e => e.GroupBy(v => v.AttributeId)
                .Select(vg => new TempAttributeWithValues
                {
                    // 2020-07-31 2dm - never used
                    // AttributeId = vg.Key,
                    Name = vg.First().Attribute.StaticName,
                    Values = vg
                        // The order of values is significant because the 2sxc system uses the first value as fallback
                        // Because we can't ensure order of values when saving, order values: prioritize values without
                        // any dimensions, then values with primary language
                        .OrderByDescending(v2 => !v2.ToSicEavValuesDimensions.Any())
                        .ThenByDescending(v2 => v2.ToSicEavValuesDimensions.Any(l =>
                            string.Equals(l.Dimension.EnvironmentKey, primaryLanguage,
                                StringComparison.InvariantCultureIgnoreCase)))
                        .ThenBy(v2 => v2.ChangeLogCreated)
                        .Select(v2 => new TempValueWithLanguage
                        {
                            Value = v2.Value,
                            Languages = v2.ToSicEavValuesDimensions
                                .Select(l => new Language(l.Dimension.EnvironmentKey, l.ReadOnly, l.DimensionId) as ILanguage)
                                .ToImmutableList(),
                        })
                }));
        return l.ReturnAsOk(attributes);
    }

    /// <summary>
    /// Get a chunk of relationships.
    /// Note that since it must check child/parents then multiple chunks could return the identical relationship.
    /// See https://github.com/2sic/2sxc/issues/2127
    /// This is why the conversion to dictionary etc. must happen later, when all chunks are merged.
    /// </summary>
    /// <returns></returns>
    private List<ToSicEavEntityRelationships> GetRelationshipChunk(int appId, ICollection<int> entityIdsFound)
    {
        var l = Log.Fn<List<ToSicEavEntityRelationships>>($"app: {appId}, ids: {entityIdsFound.Count}");
        var relationships = _dbContext.ToSicEavEntityRelationships
            .Include(rel => rel.Attribute)
            .Where(rel => rel.ParentEntity.AppId == appId)
            .Where(r => !r.ChildEntityId.HasValue // child can be a null-reference
                        || entityIdsFound.Contains(r.ChildEntityId.Value) // check if it's referred to as a child
                        || entityIdsFound.Contains(r.ParentEntityId)) // check if it's referred to as a parent
            .ToList();
        return l.ReturnAsOk(relationships);
    }

    private Dictionary<int, IEnumerable<TempRelationshipList>> GroupUniqueRelationships(IReadOnlyCollection<ToSicEavEntityRelationships> relationships)
    {
        var callLog = Log.Fn<Dictionary<int, IEnumerable<TempRelationshipList>>>($"items: {relationships.Count}", timer: true);

        Log.A("experiment!");
        var unique = relationships.Distinct(new RelationshipComparer()).ToList();
        Log.A("Distinct relationships: " + unique.Count);

        var relatedEntities = unique // relationships
            .GroupBy(g => g.ParentEntityId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(r => r.AttributeId)
                    .Select(rg => new TempRelationshipList
                    {
                        StaticName = rg.First().Attribute.StaticName,
                        Children = rg
                            .OrderBy(c => c.SortOrder)
                            .Select(c => c.ChildEntityId)
                            .ToList()
                    }));
        return callLog.ReturnAsOk(relatedEntities);
    }
}

internal class RelationshipComparer : IEqualityComparer<ToSicEavEntityRelationships>
{
    public bool Equals(ToSicEavEntityRelationships x, ToSicEavEntityRelationships y)
    {
        if (x == null && y == null) return true;
        if (x == null) return false;
        if (y == null) return false;
        return x.AttributeId == y.AttributeId
               && x.SortOrder == y.SortOrder
               && x.ParentEntityId == y.ParentEntityId
               && x.ChildEntityId == y.ChildEntityId;
    }

    public int GetHashCode(ToSicEavEntityRelationships obj)
        => obj.GetHashCode();
}