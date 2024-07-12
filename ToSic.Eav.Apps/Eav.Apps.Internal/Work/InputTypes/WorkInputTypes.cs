using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Plumbing;
using static ToSic.Eav.Data.InputTypes;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkInputTypes(
    LazySvc<IAppStates> appStates,
    LazySvc<IAppFileSystemLoader> appFileSystemLoaderLazy,
    GenWorkPlus<WorkEntities> workEntities)
    : WorkUnitBase<IAppWorkCtxPlus>("ApS.InpGet", connect: [appStates, workEntities, appFileSystemLoaderLazy])
{
    /// <summary>
    /// Retrieve a list of all input types known to the current system
    /// </summary>
    /// <returns></returns>
    public List<InputTypeInfo> GetInputTypes()
    {
        var l = Log.Fn<List<InputTypeInfo>>();

        // Initial list is the global, file-system based types
        var globalDef = GetPresetInputTypesBasedOnContentTypes();
        LogListOfInputTypes("Global", globalDef);

        // Merge input types registered in this app
        var appTypes = GetAppRegisteredInputTypes();
        LogListOfInputTypes("In App", appTypes);

        // Load input types which are stored as app-extension files
        var extensionTypes = GetAppExtensionInputTypes();

        var inputTypes = extensionTypes;
        if (inputTypes.Count > 0)
            AddMissingTypes(inputTypes, appTypes);
        else
            inputTypes = appTypes;

        AddMissingTypes(inputTypes, globalDef);
        LogListOfInputTypes("Combined", inputTypes);

        // Merge input types registered in global metadata-app
        var systemAppCtx = workEntities.CtxSvc.ContextPlus(Constants.MetaDataAppId);
        var systemAppInputTypes = GetAppRegisteredInputTypes(systemAppCtx);
        systemAppInputTypes = MarkOldGlobalInputTypesAsObsolete(systemAppInputTypes);
        LogListOfInputTypes("System", systemAppInputTypes);
        AddMissingTypes(inputTypes, systemAppInputTypes);
        LogListOfInputTypes("All combined", inputTypes);

        // Sort for better debugging
        inputTypes = [.. inputTypes.OrderBy(i => i.Type)];

        return l.Return(inputTypes, $"found {inputTypes.Count}");

        // Inner helper to log each intermediate state
        void LogListOfInputTypes(string title, List<InputTypeInfo> inputsToLog)
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
    private static void AddMissingTypes(List<InputTypeInfo> target, List<InputTypeInfo> additional)
        => additional.ForEach(sit =>
        {
            if (target.FirstOrDefault(ait => ait.Type == sit.Type) == null)
                target.Add(sit);
        });

    /// <summary>
    /// Mark obsolete InputTypes which were previously part of the installation.
    /// This is important, because the config cannot mark them as obsolete
    /// </summary>
    /// <param name="oldGlobalTypes"></param>
    /// <returns></returns>
    private List<InputTypeInfo> MarkOldGlobalInputTypesAsObsolete(List<InputTypeInfo> oldGlobalTypes) =>
        oldGlobalTypes
            .Select(it =>
            {
                it.IsObsolete = true;
                it.ObsoleteMessage = "Old input type, will default to another one.";
                return it;
            })
            .ToList();


    /// <summary>
    /// Get a list of input-types registered to the current app
    /// </summary>
    /// <param name="overrideCtx">App context to use. Often the current app, but can be a custom one.</param>
    /// <returns></returns>
    private List<InputTypeInfo> GetAppRegisteredInputTypes(IAppWorkCtxPlus overrideCtx = default)
    {
        var list = workEntities.New(overrideCtx ?? AppWorkCtx)
            .Get(TypeForInputTypeDefinition);

        return list
            .Select(e => new InputTypes(e))
            .Select(e => new InputTypeInfo(
                e.Type,
                e.Label,
                e.Description,
                e.Assets,
                e.DisableI18n,
                e.AngularAssets,
                e.UseAdam,
                "app-registered",
                e.Metadata
            ))
            .ToList();
    }


    /// <summary>
    /// Experimental v11 - load input types based on folder
    /// </summary>
    /// <returns></returns>
    private List<InputTypeInfo> GetAppExtensionInputTypes()
    {
        var l = Log.Fn<List<InputTypeInfo>>();
        try
        {
            var appLoader = appFileSystemLoaderLazy.Value.Init(AppWorkCtx.AppState);
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
    private List<InputTypeInfo> GetPresetInputTypesBasedOnContentTypes()
    {
        var l = Log.Fn<List<InputTypeInfo>>(timer: true);
        if (_presetInpTypeCache != null) return l.Return(_presetInpTypeCache, $"cached {_presetInpTypeCache.Count}");

        var presetApp = appStates.Value.GetPresetReader();

        var types = presetApp.ContentTypes
            .Where(p => p.NameId.StartsWith(FieldTypePrefix)
                        // new 16.08 experimental
                        || p.Name.StartsWith(FieldTypePrefix)
                        || p.Metadata.HasType(TypeForInputTypeDefinition) 
            )
            .Select(p => p)
            .ToList();

        // Define priority of metadata to check
        var typesToCheckInThisOrder = new[] { TypeForInputTypeDefinition, ContentTypeDetails.ContentTypeTypeName, null };
        var inputsWithAt = types.Select(it =>
            {
                var md = it.Metadata;
                return new InputTypeInfo(
                    // 2023-11-10 2dm - changed this to support new input-types based on guid-content-types
                    //it.NameId.TrimStart(FieldTypePrefix[0]),
                    md.GetBestValue<string>(nameof(InputTypes.Type), TypeForInputTypeDefinition)
                        .UseFallbackIfNoValue(GetTypeName(it)).TrimStart(FieldTypePrefix[0]),
                    md.GetBestValue<string>(nameof(InputTypes.Label), typesToCheckInThisOrder),
                    md.GetBestValue<string>(nameof(InputTypes.Description), typesToCheckInThisOrder),
                    md.GetBestValue<string>(nameof(InputTypes.Assets), TypeForInputTypeDefinition),
                    md.GetBestValue<bool>(nameof(InputTypes.DisableI18n), TypeForInputTypeDefinition),
                    md.GetBestValue<string>(nameof(InputTypes.AngularAssets), TypeForInputTypeDefinition),
                    md.GetBestValue<bool>(nameof(InputTypes.UseAdam), TypeForInputTypeDefinition),
                    "preset",
                    md
                );
            })
            .ToList();

        _presetInpTypeCache = inputsWithAt;
        return l.Return(_presetInpTypeCache, $"{_presetInpTypeCache.Count}");
    }

    private static List<InputTypeInfo> _presetInpTypeCache;

    public static string GetTypeName(IContentType t)
        => (Guid.TryParse(t.NameId, out _) ? t.Name : t.NameId).TrimStart('@');
}