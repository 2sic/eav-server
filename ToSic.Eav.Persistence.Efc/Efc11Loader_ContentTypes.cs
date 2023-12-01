using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Persistence.Efc;

partial class Efc11Loader
{
    /// <inheritdoc />
    /// <summary>
    /// Get all ContentTypes for specified AppId. 
    /// If uses temporary caching, so if called multiple times it loads from a private field.
    /// </summary>
    public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source) 
        => LoadContentTypesFromDb(appId, source);


    private IList<IContentType> LoadExtensionsTypesAndMerge(IAppStateCache app, IList<IContentType> dbTypes)
    {
        var l = Log.Fn<IList<IContentType>>(timer: true);
        try
        {
            if (string.IsNullOrEmpty(app.Folder)) return l.Return(dbTypes, "no path");

            var fileTypes = InitFileSystemContentTypes(app);
            if (fileTypes == null || fileTypes.Count == 0) return l.Return(dbTypes, "no app file types");

            Log.A($"Will check {fileTypes.Count} items");

            // remove previous items with same name, as the "static files" have precedence
            var typeToMerge = dbTypes.ToList();
            var before = typeToMerge.Count;
            var comparer = new EqualityComparer_ContentType();
            typeToMerge.RemoveAll(t => fileTypes.Contains(t, comparer));
            foreach (var fType in fileTypes)
            {
                Log.A($"Will add {fType.Name}");
                typeToMerge.Add(fType);
            }

            return l.Return(typeToMerge, $"before {before}, now {typeToMerge.Count} types");
        }
        catch (System.Exception e)
        {
            return l.Return(dbTypes, "error:" + e.Message);
        }
    }

    /// <summary>
    /// Will load file based app content-types.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    private IList<IContentType> InitFileSystemContentTypes(IAppStateCache app)
    {
        var l = Log.Fn<IList<IContentType>>();
        // must create a new loader for each app
        var loader = _appFileContentTypesLoader.New().Init(app.ToInterface(Log));
        var types = loader.ContentTypes(entitiesSource: app);
        return l.ReturnAsOk(types);
    }

    /// <summary>
    /// Load DB content-types into loader-cache
    /// </summary>
    private ImmutableList<IContentType> LoadContentTypesFromDb(int appId, IHasMetadataSource source)
    {
        // WARNING: 2022-01-18 2dm
        // I believe there is an issue which can pop up from time to time, but I'm not sure if it's only in dev setup
        // The problem is that content-types and attributes get get metadata from another app
        // That app is retrieved once needed - but the object retrieving it is given here (the AppState)
        // There seem to be cases where the following happens much later on:
        // 1. The remote MD comes from an App which hasn't been loaded yet
        // 2. The AppState has a ServiceProvider which has been destroyed
        // 3. It fails to load the App later, because the ServiceProvider is missing
        // I'm not sure if this is an issue we need to fix, but we must keep an eye on it
        // If it happens in the wild, this would probably be the solution:
        // 1. Collect AppIds used in content-types and attributes here
        // 2. After loading the types, access the app-state of each of these IDs to ensure it's loaded already

        var l = Log.Fn<ImmutableList<IContentType>>(timer: true);
        // Load from DB
        var sqlTime = Stopwatch.StartNew();
        var query = _dbContext.ToSicEavAttributeSets
            .Where(set => set.AppId == appId && set.ChangeLogDeleted == null);

        var serializer = _dataDeserializer.New();
        serializer.Initialize(appId, new List<IContentType>(), null);

        var contentTypesSql = query
            .Include(set => set.ToSicEavAttributesInSets)
            .ThenInclude(attrs => attrs.Attribute)
            .Include(set => set.App)
            .Include(set => set.UsesConfigurationOfAttributeSetNavigation)
            .ThenInclude(master => master.App)
            .ToList();

        var contentTypes = contentTypesSql
            .Select(set => new
            {
                set.AttributeSetId,
                set.Name,
                set.StaticName,
                set.Scope,
                Attributes = set.ToSicEavAttributesInSets
                    .Where(a => a.Attribute.ChangeLogDeleted == null) // only not-deleted attributes!
                    .Select(a => _dataBuilder.TypeAttributeBuilder
                        .Create(appId: appId,
                            name: a.Attribute.StaticName,
                            type: ValueTypeHelpers.Get(a.Attribute.Type), 
                            isTitle: a.IsTitle,
                            id: a.AttributeId,
                            sortOrder: a.SortOrder,
                            // #SharedFieldDefinition
                            // metadata: attrMetadata,
                            metaSourceFinder: () => source,
                            guid: a.Attribute.Guid,
                            sysSettings: serializer.DeserializeAttributeSysSettings(a.Attribute.SysSettings)
                        )),
                IsGhost = set.UsesConfigurationOfAttributeSet,
                SharedDefinitionId = set.UsesConfigurationOfAttributeSet,
                AppId = set.UsesConfigurationOfAttributeSetNavigation?.AppId ?? set.AppId,
                ZoneId = set.UsesConfigurationOfAttributeSetNavigation?.App?.ZoneId ?? set.App.ZoneId,
                ConfigIsOmnipresent =
                    set.UsesConfigurationOfAttributeSetNavigation?.AlwaysShareConfiguration ??
                    set.AlwaysShareConfiguration,
            })
            .ToList();
        sqlTime.Stop();

        var shareids = contentTypes.Select(c => c.SharedDefinitionId).ToList();
        sqlTime.Start();

        var sharedAttribs = _dbContext.ToSicEavAttributeSets
            .Include(s => s.ToSicEavAttributesInSets)
            .ThenInclude(a => a.Attribute)
            .Where(s => shareids.Contains(s.AttributeSetId))
            .ToDictionary(
                s => s.AttributeSetId,
                s => s.ToSicEavAttributesInSets.Select(a
                    => _dataBuilder.TypeAttributeBuilder.Create(
                        appId: appId,
                        name: a.Attribute.StaticName,
                        type: ValueTypeHelpers.Get(a.Attribute.Type),
                        isTitle: a.IsTitle,
                        id: a.AttributeId,
                        sortOrder: a.SortOrder,
                        // Must get own MetaSourceFinder since they come from other apps
                        metaSourceFinder: () => _appStates.GetCacheState(s.AppId),
                        // #SharedFieldDefinition
                        //guid: a.Attribute.Guid, // 2023-10-25 Tonci didn't have this, not sure why, must check before I just add. probably guid should come from the "master"
                        sysSettings: serializer.DeserializeAttributeSysSettings(a.Attribute.SysSettings)))
            );
        sqlTime.Stop();

        // Convert to ContentType-Model
        var newTypes = contentTypes.Select(set =>
        {
            var notGhost = set.IsGhost == null;

            var ctAttributes = (set.SharedDefinitionId.HasValue
                    ? sharedAttribs[set.SharedDefinitionId.Value]
                    : set.Attributes)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<IContentTypeAttribute>()
                .ToList();

            return _dataBuilder.ContentType.Create(
                appId: appId, 
                name: set.Name,
                nameId: set.StaticName, 
                id: set.AttributeSetId,
                scope: set.Scope,
                parentTypeId: set.IsGhost,
                configZoneId: set.ZoneId,
                configAppId: set.AppId,
                isAlwaysShared: set.ConfigIsOmnipresent,
                metaSourceFinder: () => notGhost ? source : _appStates.GetReader(new AppIdentity(set.ZoneId, set.AppId)).StateCache,
                attributes: ctAttributes
            );
        });

        _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);

        return l.Return(newTypes.ToImmutableList());
    }
}