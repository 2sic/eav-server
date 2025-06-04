using ToSic.Eav.Apps.Sys;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Sys;
using static ToSic.Eav.ImportExport.Sys.ImpExpConstants;

namespace ToSic.Eav.Persistence.File;

partial class FileSystemLoader
{
    private string BundlesPath => System.IO.Path.Combine(Path, AppDataFoldersConstants.BundlesFolder);


    public Dictionary<string, JsonFormat> JsonBundleBundles => field ??= GetJsonBundleBundles();

    public Dictionary<string, JsonFormat> GetJsonBundleBundles()
    {
        var l = Log.Fn<Dictionary<string, JsonFormat>>();
        // #1. check that folder exists
        if (!CheckPathExists(Path) || !CheckPathExists(BundlesPath))
            return l.Return([], "path doesn't exist");

        const string infoIfError = "couldn't read bundle-file";
        try
        {
            // #2 find all bundle files in folder and unpack/deserialize to JsonFormat
            var jsonBundles = new Dictionary<string, JsonFormat>();
            Directory.GetFiles(BundlesPath, "*" + Extension(Files.json)).OrderBy(f => f).ToList()
                .ForEach(p =>
                {
                    l.A("Loading json bundle" + p);
                    jsonBundles[p] = Serializer.UnpackAndTestGenericJsonV1(System.IO.File.ReadAllText(p));
                });
            return l.Return(jsonBundles, $"found {jsonBundles.Count}");
        }
        catch (IOException e)
        {
            l.Ex("Failed loading type - couldn't import bundle-file, IO exception", e);
            return l.Return([], "IOException");
        }
        catch (Exception e)
        {
            l.Ex($"Failed loading bundle - {infoIfError}", e);
            return l.Return([], "error");
        }
    }



    public List<ContentTypeWithEntities> ContentTypesInBundles()
    {
        var l = Log.Fn<List<ContentTypeWithEntities>>();
        if (JsonBundleBundles.All(jb => jb.Value.Bundles?.Any(b => b.ContentTypes.SafeAny()) != true))
            return l.Return([]);

        var contentTypes = JsonBundleBundles
            .SelectMany(json => BuildContentTypesInBundles(Serializer, json.Key, json.Value))
            //.Where(ct => ct != null)
            .ToList();

        l.A("ContentTypes in bundles: " + contentTypes.Count);

        return l.ReturnAsOk(contentTypes);
    }


    /// <summary>
    /// Build contentTypes from bundle json
    /// </summary>
    /// <returns></returns>
    private List<ContentTypeWithEntities> BuildContentTypesInBundles(JsonSerializer ser, string path, JsonFormat bundleJson)
    {
        var l = Log.Fn<List<ContentTypeWithEntities>>($"path: {path}");
        try
        {
            var contentTypes = ser.GetContentTypesFromBundles(bundleJson);

            var newContentTypes = contentTypes
                .Select(ct =>
                {
                    var typeWithOrigin = dataBuilder.ContentType.CreateFrom(ct.ContentType, id: ++TypeIdSeed,
                        repoType: RepoType,
                        repoAddress: path,
                        parentTypeId: EavConstants.PresetContentTypeFakeParent,
                        configZoneId: KnownAppsConstants.PresetZoneId,
                        configAppId: KnownAppsConstants.PresetAppId);
                    return new ContentTypeWithEntities { ContentType = typeWithOrigin, Entities = ct.Entities };
                })
                .ToList();

            return l.ReturnAsOk(newContentTypes);
        }
        catch (Exception e)
        {
            l.Ex("Failed building content types from bundle json", e);
            return l.Return([], "error");
        }
    }




    public List<IEntity> EntitiesInBundles(IEntitiesSource relationshipSource)
    {
        var l = Log.Fn<List<IEntity>>($"Entities in bundles");
        if (JsonBundleBundles.All(jb => jb.Value.Bundles?.Any(b => b.Entities.SafeAny()) != true))
            return l.Return([], "no bundles have entities, return none");

        var entities = JsonBundleBundles
            .SelectMany(json =>
                BuildEntitiesInBundles(Serializer, json.Key, json.Value, relationshipSource))
            .Where(entity => entity != null).ToList();

        return l.Return(entities, $"Entities in bundles: {entities.Count}");
    }




    /// <summary>
    /// Build entities from bundle json
    /// </summary>
    /// <returns></returns>
    private List<IEntity> BuildEntitiesInBundles(JsonSerializer ser, string path, JsonFormat bundleJson, IEntitiesSource relationshipSource)
    {
        var l = Log.Fn<List<IEntity>>($"Build entities from bundle json: {path}.");
        try
        {
            // WIP - Allow relationships between loaded items
            // If we are loading from a larger context, then we have a reference to a list
            // which will be repopulated later, so only create a new one if there is none
            var entities = ser.GetEntitiesFromBundles(bundleJson, relationshipSource);
            entities = entities
                .Select(e =>
                {
                    var newId = ++EntityIdSeed;
                    return dataBuilder.Entity.CreateFrom(e, id: newId, repositoryId: newId);
                })
                .ToList();
            return l.Return(entities, $"{entities.Count}");
        }
        catch (Exception e)
        {
            l.Ex("Failed building entities from bundle json", e);
            return l.Return([], "error return none");
        }
    }
}