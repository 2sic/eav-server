using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Types;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class ContentTypeRuntime : RuntimeBase
    {
        public ContentTypeRuntime(AppRuntime appRt, ILog parentLog) : base(appRt, parentLog){}

        public IEnumerable<IContentType> All => AppRT.AppState.ContentTypes;//.GetContentTypes();

        /// <summary>
        /// Gets a ContentType by Name
        /// </summary>
        /// <returns>a content-type or null if not found</returns>
        public IContentType Get(string name) => AppRT.AppState.GetContentType(name);

        // 2020-01-17 2dm removed, not in use; better to use the AppState.GetContentType directly
        ///// <summary>
        ///// Gets a ContentType by Id
        ///// </summary>
        //public IContentType Get(int contentTypeId) => AppRT.AppState.GetContentType(contentTypeId);

        public IEnumerable<IContentType> FromScope(string scope = null, bool includeAttributeTypes = false)
        {
            var set = All.Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
            if (scope != null)
                set = set.Where(p => p.Scope == scope);
            return set.OrderBy(c => c.Name);
        }

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
            var inputTypes = GetAppRegisteredInputTypes();
            Log.Add($"in app {inputTypes.Count}");
            AddMissingTypes(globalDef, inputTypes);
            Log.Add($"combined {inputTypes.Count}");

            // Merge input types registered in global metadata-app
            var systemAppRt = new AppRuntime(Constants.MetaDataAppId, true, Log);
            var systemInputTypes = systemAppRt.ContentTypes.GetAppRegisteredInputTypes();
            Log.Add($"in system {systemInputTypes.Count}");
            AddMissingTypes(systemInputTypes, inputTypes);
            Log.Add($"combined {inputTypes.Count}");

            wraplog($"found {inputTypes.Count}");
            return inputTypes;
        }

        /// <summary>
        /// Mini-helper to enhance a list with additional entries not yet contained
        /// </summary>
        /// <param name="additional"></param>
        /// <param name="target"></param>
        private static void AddMissingTypes(List<InputTypeInfo> additional, List<InputTypeInfo> target)
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
            => AppRT.Entities.Get(Constants.TypeForInputTypeDefinition)
                .Select(e => new InputTypeInfo(
                    e.GetBestValue<string>(Constants.InputTypeType),
                    e.GetBestValue<string>(Constants.InputTypeLabel),
                    e.GetBestValue<string>(Constants.InputTypeDescription),
                    e.GetBestValue<string>(Constants.InputTypeAssets),
                    e.GetBestValue<bool>(Constants.InputTypeDisableI18N),
                    e.GetBestValue<string>(Constants.InputTypeAngularAssets),
                    e.GetBestValue<string>(Constants.InputTypeAngularMode),
                    e.GetBestValue<bool>(Constants.InputTypeUseAdam)
                ))
                .ToList();

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
                    it.Metadata.GetBestValue<string>(Constants.InputTypeAngularMode, Constants.TypeForInputTypeDefinition),
                    it.Metadata.GetBestValue<bool>(Constants.InputTypeUseAdam, Constants.TypeForInputTypeDefinition)
                ))
                .ToList();
            return retyped;
        }
    }
}
