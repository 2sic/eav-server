using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Run;
using ToSic.Eav.Types;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class ContentTypeRuntime : PartOf<AppRuntime, ContentTypeRuntime>
    {
        private readonly Lazy<AppRuntime> _lazyMetadataAppRuntime;
        private readonly Lazy<IAppFileSystemLoader> _appFileSystemLoaderLazy;

        public ContentTypeRuntime(Lazy<AppRuntime> lazyMetadataAppRuntime, Lazy<IAppFileSystemLoader> appFileSystemLoaderLazy) : base("RT.ConTyp")
        {
            _lazyMetadataAppRuntime = lazyMetadataAppRuntime;
            _appFileSystemLoaderLazy = appFileSystemLoaderLazy;
        }

        public IEnumerable<IContentType> All => Parent.AppState.ContentTypes;

        /// <summary>
        /// Gets a ContentType by Name
        /// </summary>
        /// <returns>a content-type or null if not found</returns>
        public IContentType Get(string name) => Parent.AppState.GetContentType(name);
        
        /// <summary>
        /// Retrieve a list of all input types known to the current system
        /// </summary>
        /// <returns></returns>
        public List<InputTypeInfo> GetInputTypes()
        {
            var wraplog = Log.Call();
            // Initial list is the global, file-system based types
            var globalDef = GetGlobalInputTypesBasedOnContentTypes();
            Log.Add($"in global {globalDef.Count}");

            // Merge input types registered in this app
            var appTypes = GetAppRegisteredInputTypes();
            Log.Add($"in app {appTypes.Count}");

            // Load input types which are stored as app-extension files
            var extensionTypes = GetAppExtensionInputTypes();

            var inputTypes = extensionTypes;
            if (inputTypes.Count > 0)
                AddMissingTypes(inputTypes, appTypes);
            else
                inputTypes = appTypes;

            AddMissingTypes(inputTypes, globalDef);
            Log.Add($"combined {inputTypes.Count}");

            // Merge input types registered in global metadata-app
            var systemAppRt = _lazyMetadataAppRuntime.Value.Init(State.Identity(null, Constants.MetaDataAppId), true, Log);
            var systemAppInputTypes = systemAppRt.ContentTypes.GetAppRegisteredInputTypes();
            Log.Add($"in system {systemAppInputTypes.Count}");
            AddMissingTypes(inputTypes, systemAppInputTypes);
            Log.Add($"combined {inputTypes.Count}");

            wraplog($"found {inputTypes.Count}");
            return inputTypes;
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
        /// Get a list of input-types registered to the current app
        /// </summary>
        /// <returns></returns>
        private List<InputTypeInfo> GetAppRegisteredInputTypes()
            => Parent.Entities.Get(Constants.TypeForInputTypeDefinition)
                .Select(e => new InputTypeInfo(
                    e.GetBestValue<string>(Constants.InputTypeType),
                    e.GetBestValue<string>(Constants.InputTypeLabel),
                    e.GetBestValue<string>(Constants.InputTypeDescription),
                    e.GetBestValue<string>(Constants.InputTypeAssets),
                    e.GetBestValue<bool>(Constants.InputTypeDisableI18N),
                    e.GetBestValue<string>(Constants.InputTypeAngularAssets),
                    e.GetBestValue<bool>(Constants.InputTypeUseAdam)
                ))
                .ToList();

        /// <summary>
        /// Experimental v11 - load input types based on folder
        /// </summary>
        /// <returns></returns>
        private List<InputTypeInfo> GetAppExtensionInputTypes()
        {
            var wrapLog = Log.Call<List<InputTypeInfo>>();
            try
            {
                var appState = State.Get(Parent);
                var appLoader = _appFileSystemLoaderLazy.Value.Init(appState.AppId, appState.Path, Log);
                var inputTypes = appLoader.InputTypes();
                return wrapLog(null, inputTypes);
            }
            catch (Exception e)
            {
                Log.Add("Error: " + e.Message);
                return wrapLog("error", new List<InputTypeInfo>());
            }
        }

        private const string FieldTypePrefix = "@";

        /// <summary>
        /// Build a list of global (json) Content-Types and their metadata
        /// </summary>
        /// <returns></returns>
        private static List<InputTypeInfo> GetGlobalInputTypesBasedOnContentTypes()
        {
            var types = Global.AllContentTypes()
                .Where(p => p.Key.StartsWith(FieldTypePrefix))
                .Select(p => p.Value).ToList();

            // try to access metadata, if it has any
            var typesToCheckInThisOrder = new[] { Constants.TypeForInputTypeDefinition, Constants.ContentTypeTypeName, null };
            var retyped = types.Select(it => new InputTypeInfo(
                    it.StaticName.TrimStart(FieldTypePrefix[0]),
                    it.Metadata.GetBestValue<string>(Constants.InputTypeLabel, typesToCheckInThisOrder),
                    it.Metadata.GetBestValue<string>(Constants.InputTypeDescription, typesToCheckInThisOrder),
                    it.Metadata.GetBestValue<string>(Constants.InputTypeAssets, Constants.TypeForInputTypeDefinition),
                    it.Metadata.GetBestValue<bool>(Constants.InputTypeDisableI18N, Constants.TypeForInputTypeDefinition),
                    it.Metadata.GetBestValue<string>(Constants.InputTypeAngularAssets, Constants.TypeForInputTypeDefinition),
                    it.Metadata.GetBestValue<bool>(Constants.InputTypeUseAdam, Constants.TypeForInputTypeDefinition)
                ))
                .ToList();
            return retyped;
        }
    }
}
