using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Internal.Loaders;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Internal;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static ToSic.Eav.ImportExport.Internal.ImpExpConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Persistence.File;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class FileSystemLoader: ServiceBase, IContentTypeLoader
{
    private readonly Generator<JsonSerializer> _jsonSerializerGenerator;
    private readonly DataBuilder _dataBuilder;
    public int AppId = -999;

    /// <summary>
    /// Empty constructor for DI
    /// </summary>
    public FileSystemLoader(Generator<JsonSerializer> jsonSerializerGenerator, DataBuilder dataBuilder) : base($"{EavLogs.Eav}.FsLoad")
    {
        ConnectServices(
            _jsonSerializerGenerator = jsonSerializerGenerator,
            _dataBuilder = dataBuilder
        );
    }


    public FileSystemLoader Init(int appId, string path, RepositoryTypes repoType, bool ignoreMissing, IEntitiesSource entitiesSource)
    {
        var l = Log.Fn<FileSystemLoader>($"init with appId:{appId}, path:{path}, ignore:{ignoreMissing}");
        AppId = appId;
        Path = path + (path.EndsWith("\\") ? "" : "\\");
        RepoType = repoType;
        IgnoreMissingStuff = ignoreMissing;
        EntitiesSource = entitiesSource;
        return l.ReturnAsOk(this);
    }

    private string Path { get; set; }

    private bool IgnoreMissingStuff { get; set; }

    private RepositoryTypes RepoType { get; set; }

    protected IEntitiesSource EntitiesSource;

    #region json serializer
    public JsonSerializer Serializer
    {
        get
        {
            if (_ser != null) return _ser;
            _ser = _jsonSerializerGenerator.New();

            // #SharedFieldDefinition
            // Also provide AppState if possible, for new #SharedFieldDefinition
            if (EntitiesSource is IHasMetadataSource withAppState)
                _ser.DeserializationSettings = new()
                {
                    CtAttributeMetadataAppState = withAppState
                };
            _ser.Initialize(AppId, new List<IContentType>(), EntitiesSource);
            _ser.AssumeUnknownTypesAreDynamic = true;
            return _ser;
        }
    }
    private JsonSerializer _ser;

    internal void ResetSerializer(IAppState appState)
    {
        var serializer = _jsonSerializerGenerator.New().SetApp(appState);
        _ser = serializer;
    }
    internal void ResetSerializer(List<IContentType> types)
    {
        var serializer = _jsonSerializerGenerator.New();
        serializer.Initialize(AppId, types, null);
        _ser = serializer;
    }

    #endregion

    #region Queries & Configuration

    public IList<IEntity> Entities(string folder, int idSeed, DirectEntitiesSource relationships)
    {
        var l = Log.Fn<IList<IEntity>>($"Entities in {folder} with idSeed:{idSeed}");
            
        // #1. check that folder exists
        var subPath = System.IO.Path.Combine(Path, folder);
        if (!CheckPathExists(Path) || !CheckPathExists(subPath))
            return l.Return(new List<IEntity>(), "Path doesn't exist, return none");

        // #2. WIP - Allow relationships between loaded items
        // If we are loading from a larger context, then we have a reference to a list
        // which will be repopulated later, so only create a new one if there is none
        //var hasOwnRelationshipList = relationships == null;
        //l.A("hasOwnRelationshipList: " + hasOwnRelationshipList);
        //relationships = relationships ?? new List<IEntity>();
        var relationshipsSource = relationships;// new DirectEntitiesSource(relationships);

        // #3A. special case for entities in bundles.
        if (folder == FsDataConstants.BundlesFolder)
        {
            // #3A.1 load entities from files in bundles folder
            var entitiesInBundle = EntitiesInBundles(relationshipsSource).ToList();
            l.A($"Found {entitiesInBundle.Count} Entities in Bundles");

            // #3A.2 put all found entities into the source
            //if (hasOwnRelationshipList)
            //    relationships.AddRange(entitiesInBundle);

            return l.Return(entitiesInBundle, $"{entitiesInBundle.Count}");
        }

        // #3. older case with single entities
        // #3.1 find all content-type files in folder
        var jsons = Directory
            .GetFiles(subPath, $"*{Extension(Files.json)}")
            .OrderBy(f => f)
            .ToArray();

        // #3.2 load entity-items from folder
        var jsonSerializer = Serializer;
        var entities = jsons
            .Select(json => LoadAndBuildEntity(jsonSerializer, json, ++idSeed, relationshipsSource))
            .Where(entity => entity != null)
            .ToList();
        l.A("found " + entities.Count + " entities in " + folder + " folder");

        // #3.3 put all found entities into the source
        //if (hasOwnRelationshipList)
        //    relationships.AddRange(entities);

        return l.Return(entities, $"{entities.Count}");
    }

    #endregion


    #region ContentType

    public int EntityIdSeed = -1;
    public int TypeIdSeed = -1;

    public IList<IContentType> ContentTypes() => ContentTypes(AppId, null);

    /// <inheritdoc />
    /// <param name="appId">this is not used ATM - just for interface compatibility, must always be 0</param>
    /// <param name="source">this is not used ATM - just for interface compatibility</param>
    /// <returns></returns>
    public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source)
    {
        var l = Log.Fn<IList<IContentType>>($"ContentTypes in {appId}");
            
        // #1. check that folder exists
        var pathCt = ContentTypePath;
        var contentTypes = new List<IContentType>();
        if (CheckPathExists(Path) && CheckPathExists(pathCt))
        {
            // #2 find all content-type files in folder
            var jsonFiles = Directory.GetFiles(pathCt, "*" + Extension(Files.json)).OrderBy(f => f);

            // #3 load content-types from folder
            contentTypes = jsonFiles
                .Select(json => LoadAndBuildCt(Serializer, json))
                .Where(ct => ct != null)
                .ToList();
        }
        else
            l.A("path doesn't exist");

        var entityCtCount = contentTypes.Count;

        // #4 load content-types from files in bundles folder
        var bundleCts = ContentTypesInBundles();
        var bundleCtsWithoutDuplicates = bundleCts
            .Where(bundleCt => !contentTypes.Any(ct => ct.Is(bundleCt.NameId)))
            .ToList();
        contentTypes.AddRange(bundleCtsWithoutDuplicates);
        l.A($"Types in Entities: {entityCtCount}; in Bundles {bundleCts.Count}; after remove duplicates {bundleCtsWithoutDuplicates.Count}; total {contentTypes.Count}");
        return l.Return(contentTypes, $"{contentTypes.Count}");
    }

    private string ContentTypePath => System.IO.Path.Combine(Path, FsDataConstants.TypesFolder);

    /// <summary>
    /// Try to load a content-type file, but if anything fails, just return a null
    /// </summary>
    /// <returns></returns>
    private IContentType LoadAndBuildCt(JsonSerializer ser, string path)
    {
        var l = Log.Fn<IContentType>($"Path: {path}");
        var infoIfError = "couldn't read type-file";
        try
        {
            var json = System.IO.File.ReadAllText(path);

            infoIfError = "couldn't deserialize string";
            var ct = ser.DeserializeContentType(json);

            infoIfError = "couldn't set source/parent";
            ct = _dataBuilder.ContentType.CreateFrom(ct, id: ++TypeIdSeed, repoType: RepoType, parentTypeId: Constants.PresetContentTypeFakeParent, repoAddress: path);
            return l.ReturnAsOk(ct);
        }
        catch (IOException e)
        {
            l.Ex("Failed loading type - couldn't import type-file, IO exception", e);
            return l.ReturnNull();
        }
        catch (Exception e)
        {
            l.Ex($"Failed loading type - {infoIfError}", e);
            return l.ReturnNull();
        }
    }
    #endregion


    #region todo someday items

    /// <summary>
    /// Try to load an entity (for example a query-definition)
    /// If anything fails, just return a null
    /// </summary>
    /// <returns></returns>
    private IEntity LoadAndBuildEntity(JsonSerializer ser, string path, int id, IEntitiesSource relationshipSource = null)
    {
        var l = Log.Fn<IEntity>($"Loading {path}");
        try
        {
            var json = System.IO.File.ReadAllText(path);
            var entity = ser.DeserializeWithRelsWip(json, id, allowDynamic: true, skipUnknownType: false, relationshipSource);
            return l.ReturnAsOk(entity);
        }
        catch (IOException e)
        {
            l.Ex($"Failed loading type - couldn't read file on '{path}'", e);
            return l.ReturnNull();
        }
        catch (Exception e)
        {
            l.Ex($"Failed loading type - couldn't deserialize '{path}' for unknown reason.", e);
            return l.ReturnNull();
        }
    }

    #endregion

    /// <summary>
    /// Check if a path exists - if missing path is forbidden, will raise error
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private bool CheckPathExists(string path)
    {
        var l = Log.Fn<bool>($"Check path exists: {path}");
        if (Directory.Exists(path)) return l.ReturnTrue("ok");
        if (!IgnoreMissingStuff)
            throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
        l.A("path: doesn't exist, but ignore");
        return l.ReturnFalse("not found");
    }

}