using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class ContentTypeRuntime : RuntimeBase
    {
        public ContentTypeRuntime(AppRuntime app) : base(app){}

        public IEnumerable<Eav.Interfaces.IContentType> All => App.Cache.GetContentTypes();//.Values;//.Select(c => c.Value);

        /// <summary>
        /// Gets a GeontentType by Name
        /// </summary>
        public Eav.Interfaces.IContentType Get(string name) => App.Cache.GetContentType(name);

        /// <summary>
        /// Gets a ContentType by Id
        /// </summary>
        public Eav.Interfaces.IContentType Get(int contentTypeId) => App.Cache.GetContentType(contentTypeId);

        public IEnumerable<Eav.Interfaces.IContentType> FromScope(string scope = null, bool includeAttributeTypes = false)
        {
            //var contentTypes = Cache.GetContentTypes();
            var set = All 
                .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
            if (scope != null)
                set = set.Where(p => p.Scope == scope);
            return set.OrderBy(c => c.Name);
        }


        public IEnumerable<Eav.Interfaces.IEntity> GetInputTypes(bool includeGlobalDefinitions)
        {
            var inputsOfThisApp = App.Entities.Get(Constants.TypeForInputTypeDefinition).ToList();

            if (includeGlobalDefinitions)
            {
                var systemDef = new AppRuntime(Constants.MetaDataAppId);
                var systemInputTypes = systemDef.ContentTypes.GetInputTypes(false).ToList();

                systemInputTypes.ForEach(sit => {
                    if (inputsOfThisApp.FirstOrDefault(ait => ait.Title == sit.Title) == null)
                        inputsOfThisApp.Add(sit);
                });

            }
            return inputsOfThisApp;
        }


    }
}
