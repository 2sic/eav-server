using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Api.Api01;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Permissions;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static System.StringComparison;
using IEntity = ToSic.Eav.Data.IEntity;

// This is the simple API used to quickly create/edit/delete entities

// todo: there is quite a lot of duplicate code here
// like code to build attributes, or convert id-relationships to guids
// this should be in the AttributeBuilder or similar

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Api.Api01
{
    /// <summary>
    /// This is a simple controller with some Create, Update and Delete commands. 
    /// </summary>
    public partial class SimpleDataController: ServiceBase
    {
        private readonly LazySvc<EntityBuilder> _entityBuilder;

        #region Constructor / DI

        /// <summary>
        /// Used for DI - must always call Init to use
        /// </summary>
        public SimpleDataController(
            LazySvc<AttributeBuilderForImport> lazyAttributeBuilder,
            LazySvc<EntityBuilder> entityBuilder,
            LazySvc<AppManager> appManagerLazy,
            LazySvc<DbDataController> dbDataLazy,
            IZoneMapper zoneMapper,
            IContextOfSite ctx,
            Generator<AppPermissionCheck> appPermissionCheckGenerator) : base("Dta.Simple")
        {
            ConnectServices(
                _entityBuilder = entityBuilder,
                _appManagerLazy = appManagerLazy,
                _dbDataLazy = dbDataLazy,
                _zoneMapper = zoneMapper,
                _ctx = ctx,
                _appPermissionCheckGenerator = appPermissionCheckGenerator,
                AttributeBuilder = lazyAttributeBuilder
            );
        }
        private readonly LazySvc<AppManager> _appManagerLazy;
        private readonly LazySvc<DbDataController> _dbDataLazy;
        private readonly IZoneMapper _zoneMapper;
        private readonly IContextOfSite _ctx;
        private readonly Generator<AppPermissionCheck> _appPermissionCheckGenerator;

        private readonly LazySvc<AttributeBuilderForImport> AttributeBuilder;
        private DbDataController _context;
        private AppManager _appManager;
        private string _defaultLanguageCode;

        private int _appId;
        private bool _checkWritePermissions = true; // default behavior is to check write publish/draft permissions (that should happen for REST, but not for c# API)

        /// <param name="zoneId">Zone ID</param>
        /// <param name="appId">App ID</param>
        /// <param name="checkWritePermissions"></param>
        public SimpleDataController Init(int zoneId, int appId, bool checkWritePermissions = true) => Log.Func($"{zoneId}, {appId}", l =>
        {
            _appId = appId;

            // when zoneId is not that same as in current context, we need to set right site for provided zoneId
            if (_ctx.Site.ZoneId != zoneId) _ctx.Site = _zoneMapper.SiteOfZone(zoneId);

            _defaultLanguageCode = GetDefaultLanguage(zoneId);
            _context = _dbDataLazy.Value.Init(zoneId, appId);
            _appManager = _appManagerLazy.Value.Init(new AppIdentity(zoneId, appId));
            _checkWritePermissions = checkWritePermissions;
            l.A($"Default language:{_defaultLanguageCode}");
            return this;
        });

        private string GetDefaultLanguage(int zoneId) => Log.Func($"{zoneId}", () =>
        {
            var site = _zoneMapper.SiteOfZone(zoneId);
            if (site == null) return ("", "site is null");

            var usesLanguages = _zoneMapper.CulturesWithState(site).Any(c => c.IsEnabled);
            return (usesLanguages ? site.DefaultCultureCode : "", $"ok, usesLanguages:{usesLanguages}");
        });
        
        #endregion

        /// <summary>
        /// Create a new entity of the content-type specified.
        /// </summary>
        /// <param name="contentTypeName">Content-type</param>
        /// <param name="multiValues">
        ///     Values to be set collected in a dictionary. Each dictionary item is a pair of attribute 
        ///     name and value. To set references to other entities, set the attribute value to a list of 
        ///     entity ids. 
        /// </param>
        /// <param name="target"></param>
        /// <exception cref="ArgumentException">Content-type does not exist, or an attribute in values</exception>
        public IEnumerable<int> Create(string contentTypeName, IEnumerable<Dictionary<string, object>> multiValues, ITarget target = null
        ) => Log.Func($"{contentTypeName}, items: {multiValues?.Count()}, target: {target != null}", l =>
        {
            if (multiValues == null) return (null, "values were null");

            // ensure the type really exists
            var type = _appManager.Read.ContentTypes.Get(contentTypeName);
            if (type == null)
                throw Log.Ex(new ArgumentException("Error: Content type '" + contentTypeName + "' does not exist."));

            l.A($"Type {contentTypeName} found. Will build entities to save...");

            var importEntity = multiValues.Select(values => BuildEntity(type, values, target, null).Entity).ToList();

            var ids = _appManager.Entities.Save(importEntity);
            return (ids, "ok");
        });

        private (IEntity Entity, (bool Draft, bool Branch)? DraftAndBranch) BuildEntity(
            IContentType type, Dictionary<string, object> values, ITarget target, bool? existingIsPublished
        ) => Log.Func($"{type.Name}, {values?.Count}, target: {target != null}", l =>
        {
            // ensure it's case insensitive...
            values = values.ToInvariant();

            if (!values.ContainsKey(Attributes.EntityFieldGuid))
            {
                l.A("Add new generated guid, as none was provided.");
                values.Add(Attributes.EntityFieldGuid, Guid.NewGuid());
            }

            // Get owner form value dictionary (and remove it from values) because we need to provided it in entity constructor.
            string owner = null;
            if (values.ContainsKey(Attributes.EntityFieldOwner))
            {
                l.A("Get owner, when is provided.");
                owner = values[Attributes.EntityFieldOwner].ToString();
                values.Remove(Attributes.EntityFieldOwner);
            }

            // Find Guid from fields - a bit unclear why it's guaranteed to be here, probably was force-added before...
            // A clearer implementation would be better
            var eGuid = Guid.Parse(values[Attributes.EntityFieldGuid].ToString());

            // Prepare values to add
            var preparedValues = ConvertEntityRelations(values);

            var importEntity = _entityBuilder.Value.Create(appId: _appId, guid: eGuid, contentType: type, rawValues: new Dictionary<string, object>(), owner: owner, metadataFor: target);
            if (target != null) l.A("FYI: Set metadata target which was provided.");

            var draftAndBranch = AddValues(importEntity, type, preparedValues, _defaultLanguageCode, false, true,
                existingIsPublished);
            return (importEntity, draftAndBranch);
        });


        /// <summary>
        /// Update an entity specified by ID.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="values">
        ///     Values to be set collected in a dictionary. Each dictionary item is a pair of attribute 
        ///     name and value. To set references to other entities, set the attribute value to a list of 
        ///     entity ids. 
        /// </param>
        /// <exception cref="ArgumentException">Attribute in values does not exit</exception>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        public void Update(int entityId, Dictionary<string, object> values)
        {
            Log.A($"update i:{entityId}");
            var original = _appManager.AppState.List.FindRepoId(entityId);

            var import = BuildEntity(original.Type, values, null, original.IsPublished);

            _appManager.Entities.UpdateParts(entityId, import.Entity as Entity, import.DraftAndBranch);
        }


        /// <summary>
        /// Delete the entity specified by ID.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(int entityId) => _appManager.Entities.Delete(entityId);


        /// <summary>
        /// Delete the entity specified by GUID.
        /// </summary>
        /// <param name="entityGuid">Entity GUID</param>
        public void Delete(Guid entityGuid) => Delete(_context.Entities.GetMostCurrentDbEntity(entityGuid).EntityId);


        private Dictionary<string, object> ConvertEntityRelations(Dictionary<string, object> values)
        {
            Log.A("convert entity relations");
            var result = new Dictionary<string, object>();
            foreach (var value in values)
                if (value.Value is IEnumerable<int> ids)
                {
                    // The value has entity ids. For import, these must be converted to a string of guids.
                    var guids = ids.Select(id => _context.Entities.GetDbEntity(id))
                        .Select(entity => entity.EntityGuid).ToList();
                    result.Add(value.Key, string.Join(",", guids));
                }
                else if (value.Value is IEnumerable<Guid> guids)
                    result.Add(value.Key, string.Join(",", guids));
                else if (value.Value is IEnumerable<Guid?> nullGuids)
                    result.Add(value.Key, string.Join(",", nullGuids));
                else
                    result.Add(value.Key, value.Value);
            return result;
        }

        private (bool Draft, bool Branch)? AddValues(Entity entity, IContentType contentType, Dictionary<string, object> valuePairs,
            string valuesLanguage, bool valuesReadOnly, bool resolveHyperlink, bool? existingIsPublished
        ) => Log.Func($"..., ..., values: {valuePairs?.Count}, {valuesLanguage}, read-only: {valuesReadOnly}, {nameof(resolveHyperlink)}: {resolveHyperlink}", l =>
        {
            (bool Draft, bool Branch)? draftAndBranch = null;
            if (valuePairs == null)
                return (draftAndBranch, "no values");

            // On update, by default preserve IsPublished state
            if (existingIsPublished.HasValue) entity.IsPublished = existingIsPublished.Value;

            // Ensure WritePublished or WriteDraft user permissions. 
            var writePublishAllowed = GetWritePublishAllowedOrThrow(contentType);

            // IsPublished becomes false when write published is not allowed.
            if (entity.IsPublished && !writePublishAllowed) entity.IsPublished = false;

            foreach (var keyValuePair in valuePairs)
            {
                // Handle special "PublishState" attribute
                if (SaveApiAttributes.SavePublishingState.Equals(keyValuePair.Key, InvariantCultureIgnoreCase))
                {

                    draftAndBranch = IsDraft(
                        publishedState: valuePairs[SaveApiAttributes.SavePublishingState],
                        existingIsPublished: existingIsPublished,
                        writePublishAllowed);

                    if (draftAndBranch.HasValue) entity.IsPublished = draftAndBranch.Value.Item1; // published

                    l.A($"IsPublished: {entity.IsPublished}");
                    continue;
                }

                // Ignore entity guid - it's already set earlier
                if (keyValuePair.Key.ToLowerInvariant() == Attributes.EntityFieldGuid)
                {
                    l.A("entity-guid, ignore here");
                    continue;
                }

                // Handle content-type attributes
                var attribute = contentType[keyValuePair.Key];
                if (attribute != null && keyValuePair.Value != null)
                {
                    AttributeBuilder.Value.AddValueWIP(entity, attribute.Name, keyValuePair.Value,
                        attribute.Type, valuesLanguage, valuesReadOnly, resolveHyperlink);
                    l.A($"Attribute '{keyValuePair.Key}' will become '{keyValuePair.Value}' ({attribute.Type})");
                }
            }
            
            return (draftAndBranch, "done");
        });

        #region Permission Checks

        private bool GetWritePublishAllowedOrThrow(IContentType targetType)
        {
            // skip write publish/draft permission checks for c# API
            if (!_checkWritePermissions) return true;

            // this write publish/draft permission checks should happen only for REST API

            // 1. Find if user may write PUBLISHED:

            // 1.1. app permissions 
            if (_appPermissionCheckGenerator.New().ForAppInInstance(_ctx, _appManager.AppState)
                .UserMay(GrantSets.WritePublished)) return true;

            // 1.2. type permissions
            if (_appPermissionCheckGenerator.New().ForType(_ctx, _appManager.AppState, targetType)
                .UserMay(GrantSets.WritePublished)) return true;


            // 2. Find if user may write DRAFT:

            // 2.1. app permissions 
            if (_appPermissionCheckGenerator.New().ForAppInInstance(_ctx, _appManager.AppState)
                .UserMay(GrantSets.WriteDraft)) return false;

            // 2.2. type permissions
            if (_appPermissionCheckGenerator.New().ForType(_ctx, _appManager.AppState, targetType)
                .UserMay(GrantSets.WriteDraft)) return false;


            // 3. User is not allowed to update published or draft entity.
            throw new Exception("User is not allowed to update published or draft entity.");
        }


        #endregion
    }
}