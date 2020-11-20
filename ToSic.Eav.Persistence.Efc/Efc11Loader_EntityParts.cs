using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Efc.Intermediate;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {
        private int[] GetEntityIdOfPartnerEntities(int[] repositoryIds)
        {
            var wrapLog = Log.Call<int[]>();
            var relatedIds = from e in _dbContext.ToSicEavEntities
                where e.PublishedEntityId.HasValue && !e.IsPublished && repositoryIds.Contains(e.EntityId) &&
                      !repositoryIds.Contains(e.PublishedEntityId.Value) && e.ChangeLogDeleted == null
                select e.PublishedEntityId.Value;

            var combined = repositoryIds.Union(relatedIds).ToArray();

            return wrapLog("ok", combined);
        }


        private List<TempEntity> GetRawEntities(int[] entityIds, int appId, bool filterByEntityIds, string filterByType = null)
        {
            var wrapLog =
                Log.Call<List<TempEntity>>($"app: {appId}, ids: {entityIds.Length}, filter: {filterByEntityIds}");
            var query = _dbContext.ToSicEavEntities
                .Include(e => e.AttributeSet)
                // 2020-11-13 2dm - believe we're loading data here that is never used, as it's not in the returned data
                //.Include(e => e.ToSicEavValues)
                //.ThenInclude(v => v.ToSicEavValuesDimensions)
                .Where(e => !e.ChangeLogDeleted.HasValue &&
                            e.AppId == appId &&
                            e.AttributeSet.ChangeLogDeleted == null &&
                            (
                                // filter by EntityIds (if set)
                                !filterByEntityIds || entityIds.Contains(e.EntityId)
                            ));
            if (filterByType != null)
                query = query.Where(e => e.ContentType == filterByType);

            var rawEntities = query
                .OrderBy(e => e.EntityId) // order to ensure drafts are processed after draft-parents
                .Select(e => new TempEntity
                {
                    EntityId = e.EntityId,
                    EntityGuid = e.EntityGuid,
                    Version = e.Version,
                    AttributeSetId = e.AttributeSetId,
                    Metadata = new Metadata.Target
                    {
                        TargetType = e.AssignmentObjectTypeId,
                        KeyGuid = e.KeyGuid,
                        KeyNumber = e.KeyNumber,
                        KeyString = e.KeyString
                    },
                    IsPublished = e.IsPublished,
                    PublishedEntityId = e.PublishedEntityId,
                    Owner = e.Owner,
                    Modified = e.ChangeLogModifiedNavigation.Timestamp,
                    Json = e.Json,
                    // 2020-07-31 2dm - never used
                    //ContentType = e.ContentType
                })
                .ToList();
            return wrapLog("ok", rawEntities);
        }

        private Dictionary<int, IEnumerable<TempAttributeWithValues>> GetAttributesOfEntityChunk(List<int> entityIdsFound)
        {
            var wrapLog = Log.Call<Dictionary<int, IEnumerable<TempAttributeWithValues>>>(
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
                                Languages = v2.ToSicEavValuesDimensions.Select(l => new Language
                                {
                                    DimensionId = l.DimensionId,
                                    ReadOnly = l.ReadOnly,
                                    Key = l.Dimension.EnvironmentKey.ToLowerInvariant()
                                } as ILanguage).ToList(),
                            })
                    }));
            return wrapLog("ok", attributes);
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
            var wrapLog = Log.Call<List<ToSicEavEntityRelationships>>($"app: {appId}, ids: {entityIdsFound.Count}");
            var relationships = _dbContext.ToSicEavEntityRelationships
                .Include(rel => rel.Attribute)
                .Where(rel => rel.ParentEntity.AppId == appId)
                .Where(r => !r.ChildEntityId.HasValue // child can be a null-reference
                            || entityIdsFound.Contains(r.ChildEntityId.Value) // check if it's referred to as a child
                            || entityIdsFound.Contains(r.ParentEntityId)) // check if it's referred to as a parent
                .ToList();
            return wrapLog("ok", relationships);
        }

        private Dictionary<int, IEnumerable<TempRelationshipList>> GroupUniqueRelationships(IReadOnlyCollection<ToSicEavEntityRelationships> relationships)
        {
            var callLog = Log.Call($"items: {relationships.Count}", useTimer: true);

            Log.Add("experiment!");
            var unique = relationships.Distinct(new RelationshipComparer()).ToList();
            Log.Add("Distinct relationships: " + unique.Count);

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
            callLog("ok");
            return relatedEntities;
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
}
