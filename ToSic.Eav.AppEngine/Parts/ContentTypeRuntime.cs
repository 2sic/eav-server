using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class ContentTypeRuntime : RuntimeBase
    {
        public ContentTypeRuntime(AppRuntime app, Log parentLog) : base(app, parentLog){}

        public IEnumerable<IContentType> All => App.Cache.GetContentTypes();

        /// <summary>
        /// Gets a ContentType by Name
        /// </summary>
        /// <returns>a content-type or null if not found</returns>
        public IContentType Get(string name) => App.Cache.GetContentType(name);

        /// <summary>
        /// Gets a ContentType by Id
        /// </summary>
        public IContentType Get(int contentTypeId) => App.Cache.GetContentType(contentTypeId);

        public IEnumerable<IContentType> FromScope(string scope = null, bool includeAttributeTypes = false)
        {
            var set = All 
                .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
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
            var inputTypes = GetRegisteredInputTypes();

            var globalDef = GetGlobalInputTypes();
            AddMissingTypes(globalDef, inputTypes);

            var systemDef = new AppRuntime(Constants.MetaDataAppId, Log);
            var systemInputTypes = systemDef.ContentTypes.GetRegisteredInputTypes();
            AddMissingTypes(systemInputTypes, inputTypes);

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
        private List<InputTypeInfo> GetRegisteredInputTypes()
            => App.Entities.Get(Constants.TypeForInputTypeDefinition)
            .Select(e => new InputTypeInfo(
                e.GetBestValue<string>(Constants.InputTypeType), 
                e.GetBestValue<string>(Constants.InputTypeLabel),
                e.GetBestValue<string>(Constants.InputTypeDescription),
                e.GetBestValue<string>(Constants.InputTypeAssets),
                e.GetBestValue<bool>(Constants.InputTypeDisableI18N)))
            .ToList();

        private const string FieldTypePrefix = "@";

        /// <summary>
        /// Build a list of global (json) input-types
        /// </summary>
        /// <returns></returns>
        private List<InputTypeInfo> GetGlobalInputTypes()
        {
            var types = Global.AllContentTypes()
                .Where(p => p.Key.StartsWith(FieldTypePrefix))
                .Select(p => p.Value).ToList();

            var retyped = types.Select(it =>
                {
                    // try to access metadata, if it has any
                    var metadata = it.Metadata.FirstOrDefault();
                    var inputMeta = it.Metadata.FirstOrDefault(ct => ct.Type.Name == Constants.TypeForInputTypeDefinition);
                    return new InputTypeInfo(
                        it.StaticName.TrimStart(FieldTypePrefix[0]),
                        metadata?.GetBestValue<string>(Constants.InputTypeLabel),
                        metadata?.GetBestValue<string>(Constants.InputTypeDescription),
                        inputMeta?.GetBestValue<string>(Constants.InputTypeAssets),
                        inputMeta?.GetBestValue<bool>(Constants.InputTypeDisableI18N) ?? default(bool)
                    );
                })
                .ToList();
            return retyped;
        }
    }
}
