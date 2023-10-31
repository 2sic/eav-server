using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Data;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.AppSys
{
    public class AppInputTypes: ServiceBase
    {
        private readonly AppWork _appWork;
        private readonly LazySvc<IAppFileSystemLoader> _appFileSystemLoaderLazy;
        private readonly IAppStates _appStates;
        private readonly AppEntityRead _appEntities;

        public AppInputTypes(AppEntityRead appEntities, IAppStates appStates, LazySvc<IAppFileSystemLoader> appFileSystemLoaderLazy, AppWork appWork) : base("ApS.InpGet")
        {
            ConnectServices(
                _appEntities = appEntities,
                _appStates = appStates,
                _appFileSystemLoaderLazy = appFileSystemLoaderLazy,
                _appWork = appWork
            );
        }


        /// <summary>
        /// Retrieve a list of all input types known to the current system
        /// </summary>
        /// <returns></returns>
        public List<InputTypeInfo> GetInputTypes(IAppWorkCtxPlus appCtxPlus) => Log.Func(() =>
        {
            // Inner helper to log each intermediate state
            void LogListOfInputTypes(string title, List<InputTypeInfo> inputsToLog) =>
                Log.Do($"{title}, {inputsToLog.Count}", () =>
                {
                    try
                    {
                        return string.Join(",", inputsToLog.Select(it => it.Type));
                    }
                    catch (Exception)
                    {
                        return "error";
                    }
                });

            // Initial list is the global, file-system based types
            var globalDef = GetGlobalInputTypesBasedOnContentTypes();
            LogListOfInputTypes("Global", globalDef);

            // Merge input types registered in this app
            var appTypes = GetAppRegisteredInputTypes(appCtxPlus);
            LogListOfInputTypes("In App", appTypes);

            // Load input types which are stored as app-extension files
            var extensionTypes = GetAppExtensionInputTypes(appCtxPlus);

            var inputTypes = extensionTypes;
            if (inputTypes.Count > 0)
                AddMissingTypes(inputTypes, appTypes);
            else
                inputTypes = appTypes;

            AddMissingTypes(inputTypes, globalDef);
            LogListOfInputTypes("Combined", inputTypes);

            // Merge input types registered in global metadata-app
            var systemAppCtx = _appWork.ContextPlus(Constants.MetaDataAppId);
            var systemAppInputTypes = GetAppRegisteredInputTypes(systemAppCtx);
            systemAppInputTypes = MarkOldGlobalInputTypesAsObsolete(systemAppInputTypes);
            LogListOfInputTypes("System", systemAppInputTypes);
            AddMissingTypes(inputTypes, systemAppInputTypes);
            LogListOfInputTypes("All combined", inputTypes);

            // Sort for better debugging
            inputTypes = inputTypes.OrderBy(i => i.Type).ToList();

            return (inputTypes, $"found {inputTypes.Count}");
        });

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
        private List<InputTypeInfo> MarkOldGlobalInputTypesAsObsolete(List<InputTypeInfo> oldGlobalTypes)
        {
            return oldGlobalTypes.Select(it =>
            {
                it.IsObsolete = true;
                it.ObsoleteMessage = "Old input type, will default to another one.";
                return it;
            }).ToList();
        }


        /// <summary>
        /// Get a list of input-types registered to the current app
        /// </summary>
        /// <returns></returns>
        private List<InputTypeInfo> GetAppRegisteredInputTypes(IAppWorkCtxPlus appCtxPlus)
            => _appEntities.Get(appCtxPlus, InputTypes.TypeForInputTypeDefinition)
                .Select(e => new InputTypeInfo(
                    e.Value<string>(InputTypes.InputTypeType),
                    e.Value<string>(InputTypes.InputTypeLabel),
                    e.Value<string>(InputTypes.InputTypeDescription),
                    e.Value<string>(InputTypes.InputTypeAssets),
                    e.Value<bool>(InputTypes.InputTypeDisableI18N),
                    e.Value<string>(InputTypes.InputTypeAngularAssets),
                    e.Value<bool>(InputTypes.InputTypeUseAdam),
                    e.Metadata
                ))
                .ToList();



        /// <summary>
        /// Experimental v11 - load input types based on folder
        /// </summary>
        /// <returns></returns>
        private List<InputTypeInfo> GetAppExtensionInputTypes(IAppWorkCtx appCtx) => Log.Func(l =>
        {
            try
            {
                //var appState = Parent.AppState;
                var appLoader = _appFileSystemLoaderLazy.Value.Init(appCtx.AppState /*appState*/);
                var inputTypes = appLoader.InputTypes();
                return (inputTypes, $"{inputTypes.Count}");
            }
            catch (Exception e)
            {
                l.A("Error: " + e.Message);
                return (new List<InputTypeInfo>(), "error");
            }
        });

        private const string FieldTypePrefix = "@";

        /// <summary>
        /// Build a list of global (json) Content-Types and their metadata
        /// </summary>
        /// <returns></returns>
        private List<InputTypeInfo> GetGlobalInputTypesBasedOnContentTypes()
        {
            var types = _appStates.GetPresetApp().ContentTypes
                .Where(p => p.NameId.StartsWith(FieldTypePrefix))
                .Select(p => p).ToList();

            // try to access metadata, if it has any
            var typesToCheckInThisOrder = new[] { InputTypes.TypeForInputTypeDefinition, ContentTypeDetails.ContentTypeTypeName, null };
            var retyped = types.Select(it => new InputTypeInfo(
                    it.NameId.TrimStart(FieldTypePrefix[0]),
                    it.Metadata.GetBestValue<string>(InputTypes.InputTypeLabel, typesToCheckInThisOrder),
                    it.Metadata.GetBestValue<string>(InputTypes.InputTypeDescription, typesToCheckInThisOrder),
                    it.Metadata.GetBestValue<string>(InputTypes.InputTypeAssets, InputTypes.TypeForInputTypeDefinition),
                    it.Metadata.GetBestValue<bool>(InputTypes.InputTypeDisableI18N, InputTypes.TypeForInputTypeDefinition),
                    it.Metadata.GetBestValue<string>(InputTypes.InputTypeAngularAssets, InputTypes.TypeForInputTypeDefinition),
                    it.Metadata.GetBestValue<bool>(InputTypes.InputTypeUseAdam, InputTypes.TypeForInputTypeDefinition),
                    it.Metadata
                ))
                .ToList();
            return retyped;
        }
    }


}
