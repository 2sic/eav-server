using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Persistence.Sys.Loaders;
using ToSic.Eav.Sys;
using static ToSic.Eav.ImportExport.Sys.ImpExpConstants;



namespace ToSic.Eav.Persistence.File;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class FileSystemLoader(Generator<IJsonDataReader, IJsonDataReader.Options> serializerGenerator, Generator<IJsonDataWriter> jsonWriter, DataAssembler dataAssembler, ContentTypeAssemblyKit ctAssemblyKit)
    : ServiceWithSetup<FileSystemLoaderOptions>($"{EavLogs.Eav}.FsLoad", connect: [serializerGenerator, jsonWriter, dataAssembler, ctAssemblyKit]), IContentTypeLoader
{
    /// <summary>
    /// Get Options to create  new loaders with similar settings - IMPORTANT: must preserve the counts.
    /// Later should rework to
    /// 1. have counters as private variables
    /// 2. merge them into the result returned here
    /// </summary>
    public FileSystemLoaderOptions GetLatestOptions() => MyOptions;


    #region json serializer

    [field: AllowNull, MaybeNull]
    private IJsonDataReader JsonReader => field ??= serializerGenerator.New(MyOptions.JsonReaderOptions);

    #endregion

    #region Queries & Configuration

    public IList<IEntity> Entities(string folder, IEntitiesSource relationshipsSource)
    {
        var l = Log.Fn<IList<IEntity>>($"Entities in {folder} with idSeed:{MyOptions.EntityIdSeed}", timer: true);
            
        // #1. check that folder exists
        var subPath = Path.Combine(MyOptions.Path, folder);
        if (!CheckPathExists(MyOptions.Path) || !CheckPathExists(subPath))
            return l.Return([], "Path doesn't exist, return none");

        // #2. WIP - Allow relationships between loaded items
        // If we are loading from a larger context, then we have a reference to a list
        // which will be repopulated later, so only create a new one if there is none

        // #3A. special case for entities in bundles.
        if (folder == AppDataFoldersConstants.BundlesFolder)
        {
            // #3A.1 load entities from files in bundles folder
            var entitiesInBundle = EntitiesInBundles(relationshipsSource)
                .ToListOpt();
            l.A($"Found {entitiesInBundle.Count} Entities in Bundles");

            return l.Return(entitiesInBundle, $"{entitiesInBundle.Count}");
        }

        // #3. older case with single entities
        // #3.1 find all content-type files in folder
        var jsons = Directory
            .GetFiles(subPath, $"*{Extension(Files.json)}")
            .OrderBy(f => f)
            .ToListOpt();

        // TEMP: DEBUG SERIALIZER SETTINGS
        // l.A($"Serializer: '{(JsonReader as JsonDataReader)?.LogDsDetails}'");

        // #3.2 load entity-items from folder
        var entities = jsons
            .Select(json => LoadAndBuildEntity(JsonReader, json, ++MyOptions.EntityIdSeed, relationshipsSource))
            .Where(entity => entity != null)
            .Cast<IEntity>()
            .ToListOpt();
        l.A("found " + entities.Count + " entities in " + folder + " folder");

        return l.Return(entities, $"{entities.Count}");
    }

    #endregion


    #region ContentType

    public ICollection<IContentType> ContentTypes()
        => ContentTypes(MyOptions.JsonReaderOptions.AppId, null! /* unused */);

    /// <inheritdoc />
    /// <param name="appId">this is not used ATM - just for interface compatibility, must always be 0</param>
    /// <param name="source">this is not used ATM - just for interface compatibility</param>
    /// <returns></returns>
    public ICollection<IContentType> ContentTypes(int appId, IHasMetadataSourceAndExpiring source)
    {
        var l = Log.Fn<IList<IContentType>>($"ContentTypes in {appId}");
        var contentTypes = ContentTypesWithEntities().ContentTypes;
        return l.Return(contentTypes, $"{contentTypes.Count}");
    }

    public (IList<IContentType> ContentTypes, IList<IEntity> Entities) ContentTypesWithEntities()
    {
        var l = Log.Fn<(IList<IContentType> ContentTypes, IList<IEntity> Entities)>($"ContentTypes in {MyOptions.JsonReaderOptions.AppId}", timer: true);
            
        // #1. check that folder exists
        var pathCt = ContentTypePath;
        List<IContentType> contentTypes;
        if (CheckPathExists(MyOptions.Path) && CheckPathExists(pathCt))
        {
            // #2 find all content-type files in folder
            var jsonFiles = Directory
                .GetFiles(pathCt, "*" + Extension(Files.json))
                .OrderBy(f => f);

            // #3 load content-types from folder
            contentTypes = jsonFiles
                .Select(json => LoadAndBuildCt(JsonReader, json))
                .OfType<IContentType>()
                .ToList();
        }
        else
        {
            contentTypes = [];
            l.A("path doesn't exist");
        }

        var entityCtCount = contentTypes.Count;

        // #4 load content-types from files in bundles folder
        var bundlesCtAndEntities = ContentTypesInBundles();
        var bundleCts = bundlesCtAndEntities
            .Select(set => set.ContentType)
            .ToListOpt();
        var bundleCtsWithoutDuplicates = bundleCts
            .Where(bundleCt => !contentTypes.Any(ct => ct.Is(bundleCt.NameId)))
            .ToListOpt();
        contentTypes.AddRange(bundleCtsWithoutDuplicates);

        var entities = bundlesCtAndEntities
            .SelectMany(set => set.Entities)
            // ReSharper disable once RedundantEnumerableCastCall
            .OfType<IEntity>()
            .GroupBy(e => e.EntityGuid)
            .Select(g => g.First())
            .ToListOpt();

        l.A($"Types in Entities: {entityCtCount}; in Bundles {bundleCts.Count}; Bundles after remove duplicates {bundleCtsWithoutDuplicates.Count}; final: {contentTypes.Count}");
        return l.Return((contentTypes, entities), $"Content Types: {contentTypes.Count}; Entities: {entities.Count}");
    }

    private string ContentTypePath => Path.Combine(MyOptions.Path, AppDataFoldersConstants.TypesFolder);

    /// <summary>
    /// Try to load a content-type file, but if anything fails, just return a null
    /// </summary>
    /// <returns></returns>
    private IContentType? LoadAndBuildCt(IJsonDataReader ser, string path)
    {
        var l = Log.Fn<IContentType?>($"Path: {path}", timer: true);
        var infoIfError = "couldn't read type-file";
        try
        {
            var json = System.IO.File.ReadAllText(path);

            infoIfError = "couldn't deserialize string";
            var ct = ser.DeserializeContentType(json);

            infoIfError = "couldn't set source/parent";
            MyOptions.TypeIdSeed += MyOptions.TypeIdDirection;
            ct = ctAssemblyKit.Type.CreateFrom(ct, id: MyOptions.TypeIdSeed, repoType: MyOptions.RepoType, parentTypeId: EavConstants.PresetContentTypeFakeParent, repoAddress: path);
            return l.Return(ct, $"file size was: {json.Length}");
        }
#pragma warning disable CS0162 // Unreachable code detected
        // ReSharper disable HeuristicUnreachableCode
        catch (IOException e)
        {
            l.Ex($"Failed loading type - couldn't import type-file, IO exception; '{infoIfError}'", e);
#if DEBUG
            throw;
#endif
            return l.ReturnNull();
        }
        catch (Exception e)
        {
#if DEBUG
            throw;
#endif
            l.Ex($"Failed loading type - {infoIfError}", e);
            return l.ReturnNull();
        }
        // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0162 // Unreachable code detected
    }
    #endregion


    #region todo someday items

    /// <summary>
    /// Try to load an entity (for example a query-definition)
    /// If anything fails, just return a null
    /// </summary>
    /// <returns></returns>
    private IEntity? LoadAndBuildEntity(IJsonDataReader ser, string path, int id, IEntitiesSource? relationshipSource = null)
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
        if (Directory.Exists(path))
            return l.ReturnTrue("ok");
        if (!MyOptions.IgnoreMissing)
            throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
        l.A("path: doesn't exist, but ignore");
        return l.ReturnFalse("not found");
    }

}