using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps.Manage
{
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public class ContentTypeRuntime : BaseRuntime
    {
        public ContentTypeRuntime(AppRuntime app) : base(app){}

        public IEnumerable<IContentType> All => _app.Cache.GetContentTypes().Select(c => c.Value);

        /// <summary>
        /// Gets a GeontentType by Name
        /// </summary>
        public IContentType Get(string name) => _app.Cache.GetContentType(name);

        /// <summary>
        /// Gets a ContentType by Id
        /// </summary>
        public IContentType Get(int contentTypeId) => _app.Cache.GetContentType(contentTypeId);

        public IEnumerable<IContentType> Get(string scope = null, bool includeAttributeTypes = false)
        {
            //var contentTypes = Cache.GetContentTypes();
            var set = All 
                .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
            if (scope != null)
                set = set.Where(p => p.Scope == scope);
            return set.OrderBy(c => c.Name);
        }


        public IEnumerable<IEntity> GetInputTypes(bool includeGlobalDefinitions)
        {
            var inputsOfThisApp = _app.Entities.Get(Constants.TypeForInputTypeDefinition).ToList();

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
