﻿using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.PresetLoaders;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Serialization;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Persistence.Efc.Sys.Services;

internal class EfcContentTypeLoaderService(
    EfcAppLoaderService efcAppLoader,
    Generator<IAppContentTypesLoader> appFileContentTypesLoader,
    Generator<IDataDeserializer> dataDeserializer,
    DataBuilder dataBuilder,
    IAppStateCacheService appStates,
    ISysFeaturesService featuresSvc)
    : HelperBase(efcAppLoader.Log, "Efc.CtLdr")
{
    internal IImmutableList<IContentType> LoadExtensionsTypesAndMerge(IAppReader appReader, IImmutableList<IContentType> dbTypes, string? folderOrNull)
    {
        var l = Log.Fn<IImmutableList<IContentType>>(timer: true);
        try
        {
            if (string.IsNullOrEmpty(appReader.Specs.Folder))
                return l.Return(dbTypes, "no path");

            l.A($"🪵 Using LogSettings: {efcAppLoader.LogSettings}");
            var fileTypes = LoadContentTypesFromFileSystem(appReader, folderOrNull);
            if (fileTypes == null || fileTypes.Count == 0)
                return l.Return(dbTypes, "no app file types");

            l.A($"Will check {fileTypes.Count} items");

            // remove previous items with same name, as the "static files" have precedence
            var typeToMerge = dbTypes.ToImmutableOpt();
            var before = typeToMerge.Count;
            var comparer = new EqualityComparer_ContentType();
            typeToMerge = typeToMerge.RemoveAll(t => fileTypes.Contains(t, comparer));
            foreach (var fType in fileTypes)
            {
                l.A($"Will add {fType.Name}");
                typeToMerge = typeToMerge.Add(fType);
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
    private IList<IContentType> LoadContentTypesFromFileSystem(IAppReader appReader, string? folderOrNull)
    {
        var l = Log.Fn<IList<IContentType>>(timer: true);
        // must create a new loader for each app
        var loader = appFileContentTypesLoader.New();
        loader.Init(appReader, efcAppLoader.LogSettings, folderOrNull);
        var types = loader.ContentTypes(entitiesSource: appReader.GetCache());
        return l.ReturnAsOk(types);
    }

    /// <summary>
    /// Load DB content-types into loader-cache
    /// </summary>
    internal IImmutableList<IContentType> LoadContentTypesFromDb(int appId, IHasMetadataSourceAndExpiring source)
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

        var l = Log.Fn<IImmutableList<IContentType>>(timer: true);
        // Load from DB
        var sqlTime = Stopwatch.StartNew();
        var query = efcAppLoader.Context.TsDynDataContentTypes
            .Where(set => set.AppId == appId && set.TransDeletedId == null);

        var contentTypesSql = query
            .Include(set => set.TsDynDataAttributes)
            .Include(set => set.App)
            .Include(set => set.InheritContentTypeNavigation)
            .ThenInclude(master => master.App)
            .ToListOpt();

        sqlTime.Stop();

        var serializer = dataDeserializer.New();
        serializer.Initialize(appId: appId, types: [], allEntities: null);

        var contentTypes = contentTypesSql
            .Select(set => new
            {
                set.ContentTypeId,
                set.Name,
                set.StaticName,
                set.Scope,
                Attributes = set.TsDynDataAttributes
                    .Where(a => a.TransDeletedId == null) // only not-deleted attributes!
                    .Select(a => dataBuilder.TypeAttributeBuilder.Create(
                        appId: appId,
                        name: a.StaticName,
                        type: ValueTypeHelpers.Get(a.Type),
                        isTitle: a.IsTitle,
                        id: a.AttributeId,
                        sortOrder: a.SortOrder,
                        // #SharedFieldDefinition
                        // metadata: attrMetadata,
                        metaSourceFinder: () => source,
                        guid: a.Guid,
                        sysSettings: serializer.DeserializeAttributeSysSettings(a.StaticName, a.SysSettings)
                    )),
                IsGhost = set.InheritContentTypeId,
                SharedDefinitionId = set.InheritContentTypeId,
                AppId = set.InheritContentTypeNavigation?.AppId ?? set.AppId,
                ZoneId = set.InheritContentTypeNavigation?.App?.ZoneId ?? set.App.ZoneId,
                ConfigIsOmnipresent =
                    set.InheritContentTypeNavigation?.IsGlobal ??
                    set.IsGlobal,
            })
            .ToImmutableOpt();

        var optimize = featuresSvc.IsEnabled(BuiltInFeatures.SqlLoadPerformance);

        // Filter out Nulls, as they are not relevant and cause problems with Entity Framework 8.0.8
        var sharedAttribIds = optimize
            ? contentTypes
                .Where(c => c.SharedDefinitionId.HasValue)
                .Select(c => c.SharedDefinitionId!.Value)
                .Distinct()
                .ToList()
            : contentTypes
                .Select(c => c.SharedDefinitionId ?? -1)
                .ToList();

        sqlTime.Start();

        var sharedAttribs = optimize && !sharedAttribIds.Any()
            ? new()
            : efcAppLoader.Context.TsDynDataContentTypes
                .Include(s => s.TsDynDataAttributes)
                .Where(s => sharedAttribIds.Contains(s.ContentTypeId))
                .ToDictionary(
                    s => s.ContentTypeId,
                    s => s.TsDynDataAttributes.Select(a => dataBuilder.TypeAttributeBuilder.Create(
                        appId: appId,
                        name: a.StaticName,
                        type: ValueTypeHelpers.Get(a.Type),
                        isTitle: a.IsTitle,
                        id: a.AttributeId,
                        sortOrder: a.SortOrder,
                        // Must get own MetaSourceFinder since they come from other apps
                        metaSourceFinder: () => appStates.Get(s.AppId),
                        // #SharedFieldDefinition
                        //guid: a.Guid, // 2023-10-25 Tonci didn't have this, not sure why, must check before I just add. probably guid should come from the "master"
                        sysSettings: serializer.DeserializeAttributeSysSettings(a.StaticName, a.SysSettings))
                    )
                );

        sqlTime.Stop();

        // Convert to ContentType-Model
        var newTypes = contentTypes
            .Select(set =>
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
                id: set.ContentTypeId,
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

        efcAppLoader.AddSqlTime(sqlTime.Elapsed);
        var final = newTypes.ToImmutableOpt();

        return l.Return(final, $"{final.Count}");
    }

}