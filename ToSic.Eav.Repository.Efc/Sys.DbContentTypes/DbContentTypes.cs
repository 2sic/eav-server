using ToSic.Eav.Repository.Efc.Sys.DbParts;

namespace ToSic.Eav.Repository.Efc.Sys.DbContentTypes;

internal class DbContentTypes(DbStorage.DbStorage db) : DbPartBase(db, "Db.AttSet")
{
    internal IQueryable<TsDynDataContentType> GetDbContentTypeWithAttributesUntracked(int appId)
        => DbStore.SqlDb.TsDynDataContentTypes
            .AsNoTracking()
            .Include(a => a.TsDynDataAttributes)
            .Where(a => a.AppId == appId && !a.TransDeletedId.HasValue);


    internal IQueryable<TsDynDataContentType> GetDbContentTypeWithAttributesTracked(int appId)
        => DbStore.SqlDb.TsDynDataContentTypes
            .Include(a => a.TsDynDataAttributes)
            .Where(a => a.AppId == appId && !a.TransDeletedId.HasValue);


    private List<TsDynDataContentType> GetDbContentTypesUntracked(int appId, string name, bool alsoCheckNiceName = false)
    {
        var l = LogSummary.Fn<List<TsDynDataContentType>>($"{nameof(appId)}: {appId}; {nameof(name)}: {name}; {nameof(alsoCheckNiceName)}: {alsoCheckNiceName}");
        var byStaticName = GetDbContentTypeWithAttributesUntracked(appId)
            .Where(s => s.StaticName == name)
            .ToList();
        if (byStaticName.Any() || !alsoCheckNiceName)
            return l.Return(byStaticName);

        var byNiceName = GetDbContentTypeWithAttributesUntracked(appId)
            .Where(s => s.Name == name)
            .ToList();

        return l.Return(byNiceName);
    }


    internal int GetDbContentTypeId(string name)
    {
        var l = LogDetails.Fn<int>($"{nameof(name)}: {name}");
        try
        {
            var preparedError = $"too many or too few content types found for the content-type '{name}'.";
            var found = GetDbContentTypesUntracked(DbStore.AppId, name, alsoCheckNiceName: true);

            // If nothing found check parent app
            if (found.Count == 0)
            {
                // If we have exactly 1 parent, it's not inherited, so we should stop now.
                var parentAppIds = DbStore.AppIds.Skip(1).ToArray();
                if (parentAppIds.Length == 0)
                    throw l.Ex(new Exception($"{preparedError} No custom parent apps found for app {DbStore.AppId}"));

                var parentId = parentAppIds.First();
                l.A($"Not found on main app, will check parent: {parentId}");
                found = GetDbContentTypesUntracked(parentId, name, alsoCheckNiceName: true);
            }

            if (found.Count != 1)
                throw new($"{preparedError} Found {found.Count}");

            var firstId = found.First().ContentTypeId;
            return l.Return(firstId);
        }
        catch (InvalidOperationException ex)
        {
            throw new($"Unable to get Content-Type with StaticName \"{name}\" in app {DbStore.AppId}", ex);
        }
    }

    /// <summary>
    /// Test whether Content-Type exists on specified App and is not deleted
    /// </summary>
    private bool DbContentTypeExists(int appId, string staticName)
        => GetDbContentTypeWithAttributesUntracked(appId).Any(a => a.StaticName == staticName);

    internal TsDynDataContentType? PrepareDbContentType(string name, string nameId, string scope, bool skipExisting, int? appId)
    {
        if (string.IsNullOrEmpty(nameId))
            nameId = Guid.NewGuid().ToString();

        var targetAppId = appId ?? DbStore.AppId;

        // ensure Content-Type with StaticName doesn't exist on App
        if (DbStore.ContentTypes.DbContentTypeExists(targetAppId, nameId))
        {
            if (skipExisting)
                return null;
            throw new($"A Content-Type with StaticName \"{nameId}\" already exists.");
        }

        var newSet = new TsDynDataContentType
        {
            Name = name,
            StaticName = nameId,
            Scope = scope,
            TransCreatedId = DbStore.Versioning.GetTransactionId(),
            AppId = targetAppId
        };

        return newSet;
    }
        
}