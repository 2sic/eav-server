using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.BLL;

namespace ToSic.Eav.Persistence
{
    public class DbAttributeSet : BllCommandBase
    {
        public DbAttributeSet(EavDataController dc) : base(dc) { }

        /// <summary>caches all AttributeSets for each App</summary>
        internal readonly Dictionary<int, Dictionary<int, IContentType>> ContentTypes = new Dictionary<int, Dictionary<int, IContentType>>();



        /// <summary>
        /// Get a List of all AttributeSets
        /// </summary>
        public List<AttributeSet> GetAllAttributeSets()
            => Context.SqlDb.AttributeSets.Where(a => a.AppID == Context.AppId && !a.ChangeLogIDDeleted.HasValue).ToList();


        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public AttributeSet GetAttributeSet(int attributeSetId)
            => Context.SqlDb.AttributeSets.SingleOrDefault(
                    a => a.AttributeSetID == attributeSetId && a.AppID == Context.AppId && !a.ChangeLogIDDeleted.HasValue);

        /// <summary>
        /// Get a single AttributeSet
        /// </summary>
        public AttributeSet GetAttributeSet(string staticName)
            => Context.SqlDb.AttributeSets.SingleOrDefault(
                    a => a.StaticName == staticName && a.AppID == Context.AppId && !a.ChangeLogIDDeleted.HasValue);

        public AttributeSet GetAttributeSetWithEitherName(string name)
        {
            //var scopeFilter = scope?.ToString();
            var appId = Context.AppId /*_appId*/;

            try
            {
                //var test = Context.SqlDb.AttributeSets.Where(s =>
                //             s.StaticName == name && !s.ChangeLogIDDeleted.HasValue).ToList();
                var found = Context.SqlDb.AttributeSets.Where(s =>
                            s.AppID == appId
                            && s.StaticName == name
                            && !s.ChangeLogIDDeleted.HasValue
                            //&& (s.Scope == scopeFilter || scopeFilter == null)
                            ).ToList();
                // if not found, try the non-static name as fallback
                if (found.Count == 0)
                    found = Context.SqlDb.AttributeSets.Where(s =>
                            s.AppID == appId
                            && s.Name == name
                            && !s.ChangeLogIDDeleted.HasValue
                            //&& (s.Scope == scopeFilter || scopeFilter == null)
                            ).ToList();

                if (found.Count != 1)
                    throw new Exception("too many or to fewe content types found");

                return found.First();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Unable to get AttributeSet with StaticName \"" + name + "\" in app " + appId /* + " and Scope \"" + scopeFilter + "\"."*/, ex);
            }
        }

        /// <summary>
        /// if AttributeSet refers another AttributeSet, get ID of the refered AttributeSet. Otherwise returns passed AttributeSetId.
        /// </summary>
        /// <param name="attributeSetId">AttributeSetId to resolve</param>
        internal int ResolveAttributeSetId(int attributeSetId)
        {
            var usesConfigurationOfAttributeSet = Context.SqlDb.AttributeSets.Where(a => a.AttributeSetID == attributeSetId).Select(a => a.UsesConfigurationOfAttributeSet).Single();
            return usesConfigurationOfAttributeSet ?? attributeSetId;
        }

        /// <summary>
        /// Test whether AttributeSet exists on specified App and is not deleted
        /// </summary>
        public bool AttributeSetExists(string staticName, int appId)
        {
            return Context.SqlDb.AttributeSets.Any(a => !a.ChangeLogIDDeleted.HasValue && a.AppID == appId && a.StaticName == staticName);
        }



        /// <summary>
        /// Get AttributeSets
        /// </summary>
        /// <param name="appId">Filter by AppId</param>
        /// <param name="scope">optional Filter by Scope</param>
        internal IQueryable<AttributeSet> GetAttributeSets(int appId, AttributeScope? scope)
        {
            var result = Context.SqlDb.AttributeSets.Where(a => a.AppID == appId && !a.ChangeLogIDDeleted.HasValue);

            if (scope != null)
            {
                var scopeString = scope.ToString();
                result = result.Where(a => a.Scope == scopeString);
            }

            return result;
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on specified App. App must be saved and have an AppId
        /// </summary>
        internal void EnsureSharedAttributeSets(App app, bool autoSave = true)
        {
            if (app.AppID == 0)
                throw new Exception("App must have a valid AppID");

            // todo: bad - don't want data-sources here
            var sharedAttributeSets = GetAttributeSets(Constants.MetaDataAppId, null).Where(a => a.AlwaysShareConfiguration);
            foreach (var sharedSet in sharedAttributeSets)
            {
                // Skip if attributeSet with StaticName already exists
                if (app.AttributeSets.Any(a => a.StaticName == sharedSet.StaticName && !a.ChangeLogIDDeleted.HasValue))
                    continue;

                // create new AttributeSet
                var newAttributeSet = AddAttributeSet(sharedSet.Name, sharedSet.Description, sharedSet.StaticName, sharedSet.Scope, false, app.AppID);
                newAttributeSet.UsesConfigurationOfAttributeSet = sharedSet.AttributeSetID;
            }

            // Ensure new AttributeSets are created and cache is refreshed
            if (autoSave)
                Context.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on all Apps an Zones
        /// </summary>
        internal void EnsureSharedAttributeSets()
        {
            foreach (var app in Context.SqlDb.Apps)
                EnsureSharedAttributeSets(app, false);

            Context.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Add a new AttributeSet
        /// </summary>
        public AttributeSet AddAttributeSet(string name, string description, string staticName, string scope, bool autoSave = true)
            => AddAttributeSet(name, description, staticName, scope, autoSave, null);
        

        internal AttributeSet AddAttributeSet(string name, string description, string staticName, string scope, bool autoSave, int? appId)
        {
            if (string.IsNullOrEmpty(staticName))
                staticName = Guid.NewGuid().ToString();

            var targetAppId = appId ?? Context.AppId;

            // ensure AttributeSet with StaticName doesn't exist on App
            if (Context.AttribSet.AttributeSetExists(staticName, targetAppId))
                throw new Exception("An AttributeSet with StaticName \"" + staticName + "\" already exists.");

            var newSet = new AttributeSet
            {
                Name = name,
                StaticName = staticName,
                Description = description,
                Scope = scope,
                ChangeLogIDCreated = Context.Versioning.GetChangeLogId(),
                AppID = targetAppId
            };

            Context.SqlDb.AddToAttributeSets(newSet);

            if (Context.AttribSet.ContentTypes.ContainsKey(Context.AppId /* _appId*/))
                Context.AttribSet.ContentTypes.Remove(Context.AppId /* _appId*/);

            if (autoSave)
                Context.SqlDb.SaveChanges();

            return newSet;
        }
        
    }
}
