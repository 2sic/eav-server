﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Data;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class ContentTypeRuntime : PartOf<AppRuntime>
    {

        public ContentTypeRuntime(LazyInit<AppRuntime> lazyMetadataAppRuntime, LazyInit<IAppFileSystemLoader> appFileSystemLoaderLazy, IAppStates appStates) : base("RT.ConTyp") =>
            ConnectServices(
                _lazyMetadataAppRuntime = lazyMetadataAppRuntime,
                _appFileSystemLoaderLazy = appFileSystemLoaderLazy,
                _appStates = appStates
            );
        private readonly LazyInit<AppRuntime> _lazyMetadataAppRuntime;
        private readonly LazyInit<IAppFileSystemLoader> _appFileSystemLoaderLazy;
        private readonly IAppStates _appStates;

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
            // Inner helper to log each intermediate state
            void LogListOfInputTypes(string title, List<InputTypeInfo> inputsToLog)
            {
                var wrapLog2 = Log.Fn($"{title}, {inputsToLog.Count}");
                try
                {
                    wrapLog2.Done(string.Join(",", inputsToLog.Select(it => it.Type)));
                }
                catch (Exception)
                {
                    wrapLog2.Done("error");
                }
            }

            var wrapLog = Log.Fn<List<InputTypeInfo>>();

            // Initial list is the global, file-system based types
            var globalDef = GetGlobalInputTypesBasedOnContentTypes();
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
            var systemAppRt = _lazyMetadataAppRuntime.Value.Init(Constants.MetaDataAppId, true);
            var systemAppInputTypes = systemAppRt.ContentTypes.GetAppRegisteredInputTypes();
            systemAppInputTypes = MarkOldGlobalInputTypesAsObsolete(systemAppInputTypes);
            LogListOfInputTypes("System", systemAppInputTypes);
            AddMissingTypes(inputTypes, systemAppInputTypes);
            LogListOfInputTypes("All combined", inputTypes);
            
            // Sort for better debugging
            inputTypes = inputTypes.OrderBy(i => i.Type).ToList();

            return wrapLog.Return(inputTypes, $"found {inputTypes.Count}");
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
        private List<InputTypeInfo> GetAppRegisteredInputTypes()
            => Parent.Entities.Get(InputTypes.TypeForInputTypeDefinition)
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
        private List<InputTypeInfo> GetAppExtensionInputTypes()
        {
            var wrapLog = Log.Fn<List<InputTypeInfo>>();
            try
            {
                var appState = Parent.AppState;
                var appLoader = _appFileSystemLoaderLazy.Value.Init(appState, Log);
                var inputTypes = appLoader.InputTypes();
                return wrapLog.Return(inputTypes);
            }
            catch (Exception e)
            {
                Log.A("Error: " + e.Message);
                return wrapLog.Return(new List<InputTypeInfo>(), "error");
            }
        }

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
