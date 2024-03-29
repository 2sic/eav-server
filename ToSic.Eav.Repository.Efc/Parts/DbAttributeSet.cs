﻿namespace ToSic.Eav.Repository.Efc.Parts;

internal class DbAttributeSet(DbDataController db) : DbPartBase(db, "Db.AttSet")
{
    private IQueryable<ToSicEavAttributeSets> GetSetCoreQuery(int? appId = null)
        => DbContext.SqlDb.ToSicEavAttributeSets
            .Include(a => a.ToSicEavAttributesInSets)
            .ThenInclude(a => a.Attribute)
            .Where(a => a.AppId == (appId ?? DbContext.AppId) && !a.ChangeLogDeleted.HasValue);

    /// <summary>
    /// Get a single AttributeSet
    /// </summary>
    internal ToSicEavAttributeSets GetDbAttribSet(int attributeSetId)
        => GetSetCoreQuery().SingleOrDefault(a => a.AttributeSetId == attributeSetId);

    /// <summary>
    /// Get a single AttributeSet
    /// </summary>
    public ToSicEavAttributeSets GetDbAttribSet(string staticName)
        => GetSetCoreQuery().SingleOrDefault(a => a.StaticName == staticName);


    internal int GetId(string name)
    {
        try
        {
            var found = GetSetCoreQuery()
                .Where(s => s.StaticName == name)
                .ToList();

            // if not found, try the non-static name as fallback
            if (found.Count == 0)
                found = GetSetCoreQuery()
                    .Where(s => s.Name == name)
                    .ToList();

            if (found.Count != 1)
                throw new($"too many or too few content types found for the content-type {name} - found {found.Count}");

            return found.First().AttributeSetId;
        }
        catch (InvalidOperationException ex)
        {
            throw new($"Unable to get Content-Type/AttributeSet with StaticName \"{name}\" in app {DbContext.AppId}", ex);
        }
    }

    /// <summary>
    /// Test whether AttributeSet exists on specified App and is not deleted
    /// </summary>
    private bool DbAttribSetExists(int appId, string staticName)
        => GetSetCoreQuery(appId).Any(a => a.StaticName == staticName);

    internal ToSicEavAttributeSets PrepareDbAttribSet(string name, string nameId, string scope, bool skipExisting, int? appId)
    {
        if (string.IsNullOrEmpty(nameId))
            nameId = Guid.NewGuid().ToString();

        var targetAppId = appId ?? DbContext.AppId;

        // ensure AttributeSet with StaticName doesn't exist on App
        if (DbContext.AttribSet.DbAttribSetExists(targetAppId, nameId))
        {
            if (skipExisting)
                return null;
            throw new("An AttributeSet with StaticName \"" + nameId + "\" already exists.");
        }

        var newSet = new ToSicEavAttributeSets
        {
            Name = name,
            StaticName = nameId,
            Scope = scope,
            ChangeLogCreated = DbContext.Versioning.GetChangeLogId(),
            AppId = targetAppId
        };

        DbContext.SqlDb.Add(newSet);

        return newSet;
    }
        
}