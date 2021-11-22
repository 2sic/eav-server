using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {
        /// <inheritdoc />
        /// <summary>
        /// Get all ContentTypes for specified AppId. 
        /// If uses temporary caching, so if called multiple times it loads from a private field.
        /// </summary>
        public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source) 
            => LoadContentTypesIntoLocalCache(appId, source);


        private IList<IContentType> LoadExtensionsTypesAndMerge(AppState app, IList<IContentType> dbTypes)
        {
            var wrapLog = Log.Call<IList<IContentType>>(useTimer: true);
            try
            {
                if (string.IsNullOrEmpty(app.Folder)) return wrapLog("no path", dbTypes);

                var fileTypes = InitFileSystemContentTypes(app);
                if (fileTypes == null || fileTypes.Count == 0) return wrapLog("no app file types", dbTypes);

                Log.Add($"Will check {fileTypes.Count} items");

                // remove previous items with same name, as the "static files" have precedence
                var typeToMerge = dbTypes.ToList();
                var before = typeToMerge.Count;
                var comparer = new EqualityComparer_ContentType();
                typeToMerge.RemoveAll(t => fileTypes.Contains(t, comparer));
                foreach (var fType in fileTypes)
                {
                    Log.Add($"Will add {fType.Name}");
                    typeToMerge.Add(fType);
                }

                return wrapLog($"before {before}, now {typeToMerge.Count} types", typeToMerge);
            }
            catch (System.Exception e)
            {
                return wrapLog("error:" + e.Message, dbTypes);
            }
        }

        /// <summary>
        /// Will load file based app content-types.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private IList<IContentType> InitFileSystemContentTypes(AppState app)
        {
            var wrapLog = Log.Call<IList<IContentType>>();
            // must create a new loader for each app
            var loader = ServiceProvider.Build<IAppRepositoryLoader>().Init(app.AppId, app.Folder, Log);
            var types = loader.ContentTypes(app);
            return wrapLog("ok", types);
        }

        /// <summary>
        /// Load DB content-types into loader-cache
        /// </summary>
        private ImmutableList<IContentType> LoadContentTypesIntoLocalCache(int appId, 
            IHasMetadataSource source)
        {
            var wrapLog = Log.Call(useTimer: true);
            // Load from DB
            var sqlTime = Stopwatch.StartNew();
            var query = _dbContext.ToSicEavAttributeSets
                .Where(set => set.AppId == appId && set.ChangeLogDeleted == null);

            var contentTypes = query
                .Include(set => set.ToSicEavAttributesInSets)
                .ThenInclude(attrs => attrs.Attribute)
                .Include(set => set.App)
                .Include(set => set.UsesConfigurationOfAttributeSetNavigation)
                .ThenInclude(master => master.App)
                .ToList()
                .Select(set => new
                {
                    set.AttributeSetId,
                    set.Name,
                    set.StaticName,
                    set.Scope,
                    set.Description,
                    Attributes = set.ToSicEavAttributesInSets
                        .Where(a => a.Attribute.ChangeLogDeleted == null) // only not-deleted attributes!
                        .Select(a => new ContentTypeAttribute(appId, a.Attribute.StaticName, a.Attribute.Type,
                            a.IsTitle, a.AttributeId, a.SortOrder, source, metaSourceFinder: () => source)),
                    IsGhost = set.UsesConfigurationOfAttributeSet,
                    SharedDefinitionId = set.UsesConfigurationOfAttributeSet,
                    AppId = set.UsesConfigurationOfAttributeSetNavigation?.AppId ?? set.AppId,
                    ZoneId = set.UsesConfigurationOfAttributeSetNavigation?.App?.ZoneId ?? set.App.ZoneId,
                    ConfigIsOmnipresent =
                        set.UsesConfigurationOfAttributeSetNavigation?.AlwaysShareConfiguration ??
                        set.AlwaysShareConfiguration,
                })
                .ToList();
            sqlTime.Stop();

            var shareids = contentTypes.Select(c => c.SharedDefinitionId).ToList();
            sqlTime.Start();
            var sharedAttribs = _dbContext.ToSicEavAttributeSets
                .Include(s => s.ToSicEavAttributesInSets)
                .ThenInclude(a => a.Attribute)
                .Where(s => shareids.Contains(s.AttributeSetId))
                .ToDictionary(s => s.AttributeSetId, s => s.ToSicEavAttributesInSets.Select(a
                    => new ContentTypeAttribute(appId, a.Attribute.StaticName, a.Attribute.Type, a.IsTitle,
                        a.AttributeId, a.SortOrder, parentApp: s.AppId, metaSourceFinder: () => _appStates.Get(s.AppId))));
            sqlTime.Stop();

            // Convert to ContentType-Model
            var newTypes = contentTypes.Select(set =>
            {
                var notGhost = set.IsGhost == null;

                return (IContentType)new ContentType(appId, set.Name, set.StaticName, set.AttributeSetId,
                    set.Scope, set.Description, set.IsGhost, set.ZoneId, set.AppId, set.ConfigIsOmnipresent,
                    () => notGhost ? source : _appStates.Get(new AppIdentity(set.ZoneId, set.AppId))
                )
                {
                    Attributes = (set.SharedDefinitionId.HasValue
                            ? sharedAttribs[set.SharedDefinitionId.Value]
                            : set.Attributes)
                        // ReSharper disable once RedundantEnumerableCastCall
                        .Cast<IContentTypeAttribute>()
                        .ToList()
                };
            });

            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);

            wrapLog("");
            return newTypes.ToImmutableList();
        }

    }
}
