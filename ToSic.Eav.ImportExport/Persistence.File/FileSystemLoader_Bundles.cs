using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using static ToSic.Eav.ImportExport.Internal.ImpExpConstants;

namespace ToSic.Eav.Persistence.File;

partial class FileSystemLoader
{
    private string BundlesPath => System.IO.Path.Combine(Path, FsDataConstants.BundlesFolder);


    public Dictionary<string, JsonFormat> JsonBundleBundles => _jsonBundles.GetM(Log, l =>
    {
        // #1. check that folder exists
        if (!CheckPathExists(Path) || !CheckPathExists(BundlesPath))
            return (new(), "path doesn't exist");

        const string infoIfError = "couldn't read bundle-file";
        try
        {
            // #2 find all bundle files in folder and unpack/deserialize to JsonFormat
            var jsonBundles = new Dictionary<string, JsonFormat>();
            Directory.GetFiles(BundlesPath, "*" + Extension(ImpExpConstants.Files.json)).OrderBy(f => f).ToList()
                .ForEach(p =>
                {
                    l.A("Loading json bundle" + p);
                    jsonBundles[p] = Serializer.UnpackAndTestGenericJsonV1(System.IO.File.ReadAllText(p));
                });
            return (jsonBundles, $"found {jsonBundles.Count}");
        }
        catch (IOException e)
        {
            l.Ex("Failed loading type - couldn't import bundle-file, IO exception", e);
            return (new(), "IOException");
        }
        catch (Exception e)
        {
            l.Ex($"Failed loading bundle - {infoIfError}", e);
            return (new(), "error");
        }
    });
    private readonly GetOnce<Dictionary<string, JsonFormat>> _jsonBundles = new();





    public List<IContentType> ContentTypesInBundles()
    {
        var l = Log.Fn<List<IContentType>>($"ContentTypes in bundles");
        if (JsonBundleBundles.All(jb => jb.Value.Bundles?.Any(b => b.ContentTypes.SafeAny()) != true))
            return [];

        var contentTypes = JsonBundleBundles
            .SelectMany(json => BuildContentTypesInBundles(Serializer, json.Key, json.Value))
            .Where(ct => ct != null).ToList();

        l.A("ContentTypes in bundles: " + contentTypes.Count);

        return l.ReturnAsOk(contentTypes);
    }


    /// <summary>
    /// Build contentTypes from bundle json
    /// </summary>
    /// <returns></returns>
    private List<IContentType> BuildContentTypesInBundles(JsonSerializer ser, string path, JsonFormat bundleJson)
    {
        var l = Log.Fn<List<IContentType>>($"path: {path}");
        try
        {
            var contentTypes = ser.GetContentTypesFromBundles(bundleJson);

            var newContentTypes = contentTypes
                .Select(ct => _dataBuilder.ContentType.CreateFrom(ct, id: ++TypeIdSeed,
                    repoType: RepoType, repoAddress: path,
                    parentTypeId: Constants.PresetContentTypeFakeParent,
                    configZoneId: Constants.PresetZoneId,
                    configAppId: Constants.PresetAppId)
                )
                .ToList();

            return l.ReturnAsOk(newContentTypes);
        }
        catch (Exception e)
        {
            l.Ex($"Failed building content types from bundle json", e);
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
                    return _dataBuilder.Entity.CreateFrom(e, id: newId, repositoryId: newId);
                })
                .ToList();
            //entities.ForEach(e => e.ResetEntityIdAll(++EntityIdSeed));
            return l.Return(entities, $"{entities.Count}");
        }
        catch (Exception e)
        {
            l.Ex("Failed building entities from bundle json", e);
            return l.Return([], "error return none");
        }
    }
}