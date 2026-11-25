using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Persistence.File;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record FileSystemLoaderOptions
{

    public static IJsonDataReader.Options CreateJsonOptionsDyn(int appId, LogSettings? logSettings = null, ILog? log = null) =>
        new(appId, [])
        {
            AssumeUnknownTypesAreDynamic = true,
            LogSettings = logSettings ?? new(),
        };

    internal static IJsonDataReader.Options CreateJsonOptionsGlobalLoad(int appId, IEntitiesSource? entitySource, LogSettings? logSettings, ILog? log)
    {
        // Standard case, with an entities source which may not yet have an app-reader.
        var l = log.Fn<IJsonDataReader.Options>($"Create new JSON serializer, has EntitiesSource: {entitySource != null}; is desired type: {entitySource is IHasMetadataSourceAndExpiring}");
        // #SharedFieldDefinition
        // Also provide AppState if possible, for new #SharedFieldDefinition
        var deSerSettings = entitySource is IHasMetadataSourceAndExpiring withAppState
            ? new JsonDeSerializationSettings
            {
                MetadataSource = withAppState,
                // Just a note: this will only apply to attribute-metadata with related entities
                // but won't cover Content-Type metadata with related entities
                // ATM this doesn't matter, because we don't have any related entities in Content-Types
                // if we ever need it, check out how it's done on the AppLoader
            }
            : null;
        var jrOptions = new IJsonDataReader.Options(appId, [])
        {
            DeserializationSettings = deSerSettings,
            AssumeUnknownTypesAreDynamic = true,
            LazyRelationshipLookupList = entitySource,
            LogSettings = logSettings ?? new(),
        };

        return l.Return(jrOptions);
    }

    public required IJsonDataReader.Options JsonReaderOptions { get; init; }

    //public required int AppId { get; init; }

    public required string Path
    {
        get;
        init => field = value.ToSystemPath().TrimEnd('\\', '/') + System.IO.Path.DirectorySeparatorChar;
    }
    public required RepositoryTypes RepoType { get; init; }
    public bool IgnoreMissing { get; init; }

    //public IEntitiesSource? EntitiesSource { get; init; }

    public LogSettings LogSettings { get; init; } = new();


    /// <summary>
    /// WIP: currently still settable, but not perfect.
    /// Afterward, the ideal implementation should be that the true ID is tracked in the FileSystemLoader,
    /// and it would export new options for the next loader
    /// </summary>
    public int EntityIdSeed { get; set; } = -1;

    public int EntityIdDirection { get; init; } = 1;

    /// <summary>
    /// WIP: currently still settable, but not perfect.
    /// Afterward, the ideal implementation should be that the true ID is tracked in the FileSystemLoader,
    /// and it would export new options for the next loader
    /// </summary>
    public int TypeIdSeed { get; set; } = -1;
    public int TypeIdDirection { get; init; } = 1;

}
