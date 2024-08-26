using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Persistence.Efc;

internal class ContentTypeLoader(EfcAppLoader appLoader, Generator<IAppContentTypesLoader> appFileContentTypesLoader, Generator<IDataDeserializer> dataDeserializer, DataBuilder dataBuilder, IAppStateCacheService appStates)
    : HelperBase(appLoader.Log, "Efc.CtLdr")
{
    internal IList<IContentType> LoadExtensionsTypesAndMerge(IAppReader appReader, IList<IContentType> dbTypes)
    {
        var l = Log.Fn<IList<IContentType>>(timer: true);
        try
        {
            if (string.IsNullOrEmpty(appReader.Specs.Folder))
                return l.Return(dbTypes, "no path");

            var fileTypes = InitFileSystemContentTypes(appReader);
            if (fileTypes == null || fileTypes.Count == 0)
                return l.Return(dbTypes, "no app file types");

            l.A($"Will check {fileTypes.Count} items");

            // remove previous items with same name, as the "static files" have precedence
            var typeToMerge = dbTypes.ToList();
            var before = typeToMerge.Count;
            var comparer = new EqualityComparer_ContentType();
            typeToMerge.RemoveAll(t => fileTypes.Contains(t, comparer));
            foreach (var fType in fileTypes)
            {
                l.A($"Will add {fType.Name}");
                typeToMerge.Add(fType);
            }

            return l.Return(typeToMerge, $"before {before}, now {typeToMerge.Count} types");
        }
        catch (Exception e)
        {
            return l.Return(dbTypes, "error:" + e.Message);
        }
    }

    /// <summary>
    /// Will load file based app content-types.
    /// </summary>
    /// <returns></returns>
    private IList<IContentType> InitFileSystemContentTypes(IAppReader appReader)
    {
        var l = Log.Fn<IList<IContentType>>(timer: true);
        // must create a new loader for each app
        var loader = appFileContentTypesLoader.New().Init(appReader);
        var types = loader.ContentTypes(entitiesSource: appReader.GetCache());
        return l.ReturnAsOk(types);
    }

    /// <summary>
    /// Load DB content-types into loader-cache
    /// </summary>
    internal ImmutableList<IContentType> LoadContentTypesFromDb(int appId, IHasMetadataSourceAndExpiring source)
    {
        // WARNING: 2022-01-18 2dm
        // I believe there is an issue which can pop up from time to time, but I'm not sure if it's only in dev setup
        // The problem is that content-types and attributes get metadata from another app
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
        var query = appLoader.Context.ToSicEavAttributeSets
            .Where(set => set.AppId == appId && set.ChangeLogDeleted == null);

        var serializer = dataDeserializer.New();
        serializer.Initialize(appId, [], null);

        var contentTypesSql = query
            .Include(set => set.ToSicEavAttributesInSets)
            .ThenInclude(attrs => attrs.Attribute)
            .Include(set => set.App)
            .Include(set => set.UsesConfigurationOfAttributeSetNavigation)
            .ThenInclude(master => master.App)
            .ToList();

        sqlTime.Stop();

        var contentTypes = contentTypesSql
            .Select(set => new
            {
                set.AttributeSetId,
                set.Name,
                set.StaticName,
                set.Scope,
                Attributes = set.ToSicEavAttributesInSets
                    .Where(a => a.Attribute.ChangeLogDeleted == null) // only not-deleted attributes!
                    .Select(a => dataBuilder.TypeAttributeBuilder
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

        var sharedAttribIds = contentTypes
            .Select(c => c.SharedDefinitionId)
            .ToList();

        sqlTime.Start();

        var sharedAttribs = appLoader.Context.ToSicEavAttributeSets
            .Include(s => s.ToSicEavAttributesInSets)
            .ThenInclude(a => a.Attribute)
            .Where(s => sharedAttribIds.Contains(s.AttributeSetId))
            .ToDictionary(
                s => s.AttributeSetId,
                s => s.ToSicEavAttributesInSets.Select(a
                    => dataBuilder.TypeAttributeBuilder.Create(
                        appId: appId,
                        name: a.Attribute.StaticName,
                        type: ValueTypeHelpers.Get(a.Attribute.Type),
                        isTitle: a.IsTitle,
                        id: a.AttributeId,
                        sortOrder: a.SortOrder,
                        // Must get own MetaSourceFinder since they come from other apps
                        metaSourceFinder: () => appStates.Get(s.AppId),
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

            return dataBuilder.ContentType.Create(
                appId: appId,
                name: set.Name,
                nameId: set.StaticName,
                id: set.AttributeSetId,
                scope: set.Scope,
                parentTypeId: set.IsGhost,
                configZoneId: set.ZoneId,
                configAppId: set.AppId,
                isAlwaysShared: set.ConfigIsOmnipresent,
                // 2024-05-16 2dm changing to not use a Reader, as it's not needed and may cause #IServiceProviderDisposedException
                //metaSourceFinder: () => notGhost ? source : appStates.GetReader(new AppIdentity(set.ZoneId, set.AppId)).StateCache,
                metaSourceFinder: notGhost
                    ? () => source
                    : () => appStates.Get(new AppIdentity(set.ZoneId, set.AppId)),
                attributes: ctAttributes
            );
        });

        appLoader.AddSqlTime(sqlTime.Elapsed);
        var final = newTypes.ToImmutableList();

        return l.Return(final, $"{final.Count}");
    }

    //private static Func<IHasMetadataSource> GetMetaSourceFinder(IAppStates appStates, IAppIdentity appId)
    //    => () => appStates.Get(appId);
}