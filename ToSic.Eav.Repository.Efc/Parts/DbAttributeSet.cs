namespace ToSic.Eav.Repository.Efc.Parts;

internal class DbAttributeSet(DbDataController db) : DbPartBase(db, "Db.AttSet")
{
    private IQueryable<ToSicEavAttributeSets> GetDbContentTypeCoreQuery(int appId)
        => DbContext.SqlDb.ToSicEavAttributeSets
            .Include(a => a.ToSicEavAttributesInSets)
            .ThenInclude(a => a.Attribute)
            .Where(a => a.AppId == appId && !a.ChangeLogDeleted.HasValue);

    /// <summary>
    /// Get a single AttributeSet
    /// </summary>
    internal ToSicEavAttributeSets GetDbContentType(int appId, int attributeSetId)
        => GetDbContentTypeCoreQuery(appId).SingleOrDefault(a => a.AttributeSetId == attributeSetId);

    /// <summary>
    /// Get a single AttributeSet
    /// </summary>
    public ToSicEavAttributeSets GetDbContentType(int appId, string name, bool alsoCheckNiceName = false)
    {
        var byStaticName = GetDbContentTypeCoreQuery(appId).SingleOrDefault(a => a.StaticName == name);
        if (byStaticName != null || !alsoCheckNiceName)
            return byStaticName;
        return GetDbContentTypeCoreQuery(appId).SingleOrDefault(a => a.Name == name);
    }

    private List<ToSicEavAttributeSets> GetDbContentTypes(int appId, string name, bool alsoCheckNiceName = false)
    {
        var l = Log.Fn<List<ToSicEavAttributeSets>>($"{nameof(appId)}: {appId}; {nameof(name)}: {name}; {nameof(alsoCheckNiceName)}: {alsoCheckNiceName}");
        var byStaticName = GetDbContentTypeCoreQuery(appId)
            .Where(s => s.StaticName == name)
            .ToList();
        if (byStaticName.Any() || !alsoCheckNiceName)
            return l.Return(byStaticName);

        var byNiceName = GetDbContentTypeCoreQuery(appId)
            .Where(s => s.Name == name)
            .ToList();

        return l.Return(byNiceName);
    }


    internal int GetDbContentTypeId(string name)
    {
        var l = Log.Fn<int>($"{nameof(name)}: {name}");
        try
        {
            var preparedError = $"too many or too few content types found for the content-type '{name}'.";
            var found = GetDbContentTypes(DbContext.AppId, name, alsoCheckNiceName: true);

            // If nothing found check parent app
            if (found.Count == 0)
            {
                // If we have exactly 1 parent, it's not inherited, so we should stop now.
                var parentAppIds = DbContext.AppIds.Skip(1).ToArray();
                if (parentAppIds.Length == 0)
                    throw l.Ex(new Exception($"{preparedError} No custom parent apps found for app {DbContext.AppId}"));

                var parentId = parentAppIds.First();
                l.A($"Not found on main app, will check parent: {parentId}");
                found = GetDbContentTypes(parentId, name, alsoCheckNiceName: true);
            }

            //var found = GetSetCoreQuery(appId)
            //    .Where(s => s.StaticName == name)
            //    .ToList();

            //// if not found, try the non-static name as fallback
            //if (found.Count == 0)
            //    found = GetSetCoreQuery(appId)
            //        .Where(s => s.Name == name)
            //        .ToList();

            if (found.Count != 1)
                throw new($"{preparedError} Found {found.Count}");

            var firstId = found.First().AttributeSetId;
            return l.Return(firstId);
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
        => GetDbContentTypeCoreQuery(appId).Any(a => a.StaticName == staticName);

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