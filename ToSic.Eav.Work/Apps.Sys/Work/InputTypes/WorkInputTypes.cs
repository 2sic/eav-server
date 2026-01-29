using ToSic.Eav.Apps.Sys.FileSystemState;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.InputTypes;
using ToSic.Sys.Utils;
using static ToSic.Eav.Data.Sys.InputTypes.InputTypeDefinition;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkInputTypes(
    LazySvc<IAppReaderFactory> appReaders,
    LazySvc<IAppInputTypesLoader> appFileSystemLoaderLazy,
    LazySvc<AppWorkContextService> ctxSvc)
    : WorkUnitBase<IAppWorkCtxPlus>("ApS.InpGet", connect: [appReaders, appFileSystemLoaderLazy, ctxSvc])
{
    /// <summary>
    /// Retrieve a list of all input types known to the current system
    /// </summary>
    /// <returns></returns>
    public ICollection<InputTypeInfo> GetInputTypes()
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();

        // Initial list is the global, file-system based types
        var globalDef = GetPresetInputTypesBasedOnContentTypes();
        LogListOfInputTypes("Input Types Global", globalDef);

        // Merge input types registered in this app
        var appTypes = GetAppRegisteredInputTypes();
        LogListOfInputTypes("Input Types In App Data", appTypes);

        // Load input types which are stored as app-extension files
        var extensionTypes = GetAppExtensionInputTypes();
        LogListOfInputTypes("Input Types In App Folder", appTypes);

        // Merge all together
        var inputTypes = extensionTypes;
        inputTypes = inputTypes.Count == 0
            ? appTypes
            : AddMissingTypes(inputTypes, appTypes);

        inputTypes = AddMissingTypes(inputTypes, globalDef);
        LogListOfInputTypes("Combined", inputTypes);

        // Merge input types registered in global metadata-app
        var systemAppCtx = ctxSvc.Value.ContextPlus(KnownAppsConstants.MetaDataAppId);
        var systemAppInputTypes = GetAppRegisteredInputTypes(systemAppCtx);
        systemAppInputTypes = MarkOldGlobalInputTypesAsObsolete(systemAppInputTypes);
        LogListOfInputTypes("Input Types in System App", systemAppInputTypes);
        inputTypes = AddMissingTypes(inputTypes, systemAppInputTypes);
        LogListOfInputTypes("All combined", inputTypes);

        // Sort for better debugging
        inputTypes = [.. inputTypes.OrderBy(i => i.Type)];

        return l.Return(inputTypes, $"found {inputTypes.Count}");

        // Inner helper to log each intermediate state
        void LogListOfInputTypes(string title, ICollection<InputTypeInfo> inputsToLog)
        {
            var lInner = Log.Fn($"{title}, {inputsToLog.Count}");
            try
            {
                lInner.Done(string.Join(",", inputsToLog.Select(it => it.Type)));
            }
            catch (Exception)
            {
                lInner.Done("error");
            }
        }
    }

    /// <summary>
    /// Mini-helper to enhance a list with additional entries not yet contained
    /// </summary>
    /// <param name="target"></param>
    /// <param name="additional"></param>
    private static ICollection<InputTypeInfo> AddMissingTypes(ICollection<InputTypeInfo> target, ICollection<InputTypeInfo> additional)
    {
        var toAdd = additional
            .Where(sit => target
                .FirstOrDefault(ait => ait.Type == sit.Type) == null
            );
        return target
            .Union(toAdd)
            .ToListOpt();
    }

    /// <summary>
    /// Mark obsolete InputTypes which were previously part of the installation.
    /// This is important, because the config cannot mark them as obsolete
    /// </summary>
    /// <param name="oldGlobalTypes"></param>
    /// <returns></returns>
    private ICollection<InputTypeInfo> MarkOldGlobalInputTypesAsObsolete(ICollection<InputTypeInfo> oldGlobalTypes) =>
        oldGlobalTypes
            .Select(it =>
            {
                it.IsObsolete = true;
                it.ObsoleteMessage = "Old input type, will default to another one.";
                return it;
            })
            .ToListOpt();


    /// <summary>
    /// Get a list of input-types registered to the current app
    /// </summary>
    /// <param name="overrideCtx">App context to use. Often the current app, but can be a custom one.</param>
    /// <returns></returns>
    private ICollection<InputTypeInfo> GetAppRegisteredInputTypes(IAppWorkCtxPlus? overrideCtx = default) =>
        (overrideCtx ?? AppWorkCtx)
        .AppReader.List
        .GetAll<InputTypeDefinition>()
        .Select(e => new InputTypeInfo(metadata: (e as ICanBeEntity)?.Entity.Metadata)
        {
            Type = e.Type,
            Label = e.Label,
            Description = e.Description,
            DisableI18n = e.DisableI18n,
            UiAssets = new Dictionary<string, string> { { InputTypeInfo.DefaultAssets, e.AngularAssets ?? "" } },
            UseAdam = e.UseAdam,
            Source = "app-registered",
        })
        .ToListOpt();


    /// <summary>
    /// Experimental v11 - load input types based on folder
    /// </summary>
    /// <returns></returns>
    private ICollection<InputTypeInfo> GetAppExtensionInputTypes()
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();
        try
        {
            var appLoader = appFileSystemLoaderLazy.Value;
            appLoader.Init(AppWorkCtx.AppReader, new());
            var inputTypes = appLoader.InputTypes();
            return l.Return(inputTypes, $"{inputTypes.Count}");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.Return([], "error");
        }
    }

    private const string FieldTypePrefix = "@";

    /// <summary>
    /// Build a list of global (json) Content-Types and their metadata
    /// </summary>
    /// <returns></returns>
    private ICollection<InputTypeInfo> GetPresetInputTypesBasedOnContentTypes()
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>(timer: true);
        if (_presetInpTypeCache != null)
            return l.Return(_presetInpTypeCache, $"cached {_presetInpTypeCache.Count}");

        var presetApp = appReaders.Value.GetSystemPreset();

        var types = presetApp.ContentTypes
            .Where(p =>
                // Either NameId (old, should be guid for newer) or the Name (new v16.08) start with @
                p.NameId.StartsWith(FieldTypePrefix)
                || p.Name.StartsWith(FieldTypePrefix)
                // or they have specific metadata marking them as input-type-definitions
                || p.Metadata.HasType(ContentTypeNameId)
            )
            .Select(p => p)
            .ToListOpt();

        // Temp 2dm
        var spectrumType = presetApp.ContentTypes
            .FirstOrDefault(p => p.Name == "@string-app-color-picker-spectrum-pro");
        l.A("2dm: found spectrum type: " + (spectrumType != null));

        var typesWithMetadata = presetApp.ContentTypes
            .Where(p => p.Metadata.HasType(ContentTypeNameId))
            .ToListOpt();
        l.A("2dm: found spectrum type based on metadata: " + typesWithMetadata.Count);

        // Define priority of metadata to check
        var typesToCheckInThisOrder = new[] { ContentTypeNameId, ContentTypeDetails.ContentTypeTypeName, null };
        var inputsWithAt = types
            .Select(it =>
            {
                var md = it.Metadata;

                // 2025-11-20 2dm - preparing to have multiple editions in assets
                var defaultAssets = md.Get<string>(nameof(InputTypeDefinition.AngularAssets), typeName: ContentTypeNameId);

                // 2023-11-10 2dm - changed this to support new input-types based on guid-content-types
                return new InputTypeInfo(metadata: md)
                {
                    Type = md.Get<string>(nameof(InputTypeDefinition.Type), typeName: ContentTypeNameId)
                        .UseFallbackIfNoValue(GetTypeName(it))
                        .TrimStart(FieldTypePrefix[0]),
                    Label = md.Get<string>(nameof(InputTypeDefinition.Label), typeNames: typesToCheckInThisOrder),
                    Description = md.Get<string>(nameof(InputTypeDefinition.Description), typeNames: typesToCheckInThisOrder),
                    DisableI18n = md.Get<bool>(nameof(InputTypeDefinition.DisableI18n), typeName: ContentTypeNameId),
                    UiAssets = new Dictionary<string, string>
                    {
                        { InputTypeInfo.DefaultAssets, defaultAssets ?? "" }
                    },
                    UseAdam = md.Get<bool>(nameof(InputTypeDefinition.UseAdam), typeName: ContentTypeNameId),
                    Source = "preset",
                };
            })
            .ToListOpt();

        _presetInpTypeCache = inputsWithAt;
        return l.Return(_presetInpTypeCache, $"{_presetInpTypeCache.Count}");
    }

    private static ICollection<InputTypeInfo>? _presetInpTypeCache;

    public static string GetTypeName(IContentType t)
        => (Guid.TryParse(t.NameId, out _)
                ? t.Name
                : t.NameId)
            .TrimStart('@');
}