using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Efc.Intermediate;

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
                .Include(e => e.ToSicEavValues)
                .ThenInclude(v => v.ToSicEavValuesDimensions)
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

            var attributes = _dbContext.ToSicEavValues
                .Include(v => v.Attribute)
                .Include(v => v.ToSicEavValuesDimensions)
                .ThenInclude(d => d.Dimension)
                .Where(r => entityIdsFound.Contains(r.EntityId))
                .Where(v => !v.ChangeLogDeleted.HasValue)
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
                                string.Equals(l.Dimension.EnvironmentKey, PrimaryLanguage,
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

        private Dictionary<int, IEnumerable<TempRelationshipList>> GetRelatedEntities(int appId, List<int> entityIdsFound)
        {
            var wrapLog = Log.Call<Dictionary<int, IEnumerable<TempRelationshipList>>>(
                    $"app: {appId}, ids: {entityIdsFound.Count}");

            var relatedEntities = _dbContext.ToSicEavEntityRelationships
                .Include(rel => rel.Attribute)
                .Where(rel => rel.ParentEntity.AppId == appId)
                .Where(r => !r.ChildEntityId.HasValue ||
                            entityIdsFound.Contains(r.ChildEntityId.Value) ||
                            entityIdsFound.Contains(r.ParentEntityId))
                .GroupBy(g => g.ParentEntityId)
                .ToDictionary(g => g.Key, g => g.GroupBy(r => r.AttributeId)
                    .Select(rg => new TempRelationshipList
                    {
                        // 2020-07-31 2dm - never used
                        // AttributeId = rg.Key,
                        StaticName = rg.First().Attribute.StaticName,
                        Children = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityId)
                    }));
            return wrapLog("ok", relatedEntities);
        }
    }
}
