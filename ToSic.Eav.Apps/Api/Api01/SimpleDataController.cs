using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Api.Api01;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Generics;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Permissions;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static System.StringComparer;
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

        #region Constructor / DI

        /// <summary>
        /// Used for DI - must always call Init to use
        /// </summary>
        public SimpleDataController(
            DataBuilder builder,
            LazySvc<AppWork> appWork,
            LazySvc<AppManager> appManagerLazy,
            LazySvc<DbDataController> dbDataLazy,
            IZoneMapper zoneMapper,
            IContextOfSite ctx,
            Generator<AppPermissionCheck> appPermissionCheckGenerator) : base("Dta.Simple")
        {
            ConnectServices(
                _appWork = appWork,
                _appManagerLazy = appManagerLazy,
                _dbDataLazy = dbDataLazy,
                _zoneMapper = zoneMapper,
                _builder = builder,
                _ctx = ctx,
                _appPermissionCheckGenerator = appPermissionCheckGenerator
            );
        }
        private readonly LazySvc<AppWork> _appWork;
        private LazySvc<AppManager> _appManagerLazy { get; }
        private readonly LazySvc<DbDataController> _dbDataLazy;
        private readonly IZoneMapper _zoneMapper;
        private readonly IContextOfSite _ctx;
        private readonly Generator<AppPermissionCheck> _appPermissionCheckGenerator;
        private readonly DataBuilder _builder;


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
            var appIdentity = new AppIdentity(zoneId, appId);
            AppSysCtx = _appWork.Value.Context(appIdentity);
            _context = _dbDataLazy.Value.Init(zoneId, appId);
            _appManager = _appManagerLazy.Value.Init(appIdentity);
            _checkWritePermissions = checkWritePermissions;
            l.A($"Default language:{_defaultLanguageCode}");
            return this;
        });

        private IAppWorkCtx AppSysCtx;

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
        /// <exception cref="ArgumentException">Content-type does not exist, or an attribute in attributes</exception>
        public IEnumerable<int> Create(string contentTypeName, IEnumerable<Dictionary<string, object>> multiValues, ITarget target = null
        ) => Log.Func($"{contentTypeName}, items: {multiValues?.Count()}, target: {target != null}", l =>
        {
            if (multiValues == null) return (null, "attributes were null");

            // ensure the type really exists
            var type = AppSysCtx.AppState.GetContentType(contentTypeName);
            if (type == null)
                throw l.Done(new ArgumentException("Error: Content type '" + contentTypeName + "' does not exist."));

            l.A($"Type {contentTypeName} found. Will build entities to save...");

            var importEntity = multiValues.Select(values => BuildNewEntity(type, values, target, null).Entity).ToList();

            // #ExtractEntitySave - verified
            var ids = _appWork.Value.EntitySave(AppSysCtx.AppState).Save(importEntity);

            return (ids, "ok");
        });

        private (IEntity Entity, (bool ShouldPublish, bool DraftShouldBranch)? DraftAndBranch) BuildNewEntity(
            IContentType type,
            Dictionary<string, object> values,
            ITarget targetOrNull,
            bool? existingIsPublished
        ) => Log.Func($"{type.Name}, {values?.Count}, target: {targetOrNull != null}", l =>
        {
            // We're going to make changes to the dictionary, so we MUST copy it first, so we don't affect upstream code
            // also ensure it's case insensitive...
            values = values.ToInvariantCopy();

            if (!values.ContainsKey(Attributes.EntityFieldGuid))
            {
                l.A("Add new generated guid, as none was provided.");
                values.Add(Attributes.EntityFieldGuid, Guid.NewGuid());
            }

            // Get owner form value dictionary (and remove it from attributes) because we need to provided it in entity constructor.
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

            // Figure out publishing before converting to IAttribute
            var publishing = FigureOutPublishing(type, values, existingIsPublished);

            // Prepare attributes to add
            var preparedValues = ConvertRelationsToNullArray(type, values);
            var preparedIAttributes = _builder.Attribute.Create(preparedValues);
            var attributes = BuildNewEntityValues(type, preparedIAttributes, _defaultLanguageCode);

            var newEntity = _builder.Entity.Create(appId: _appId, guid: eGuid, contentType: type,
                attributes: _builder.Attribute.Create(attributes),
                owner: owner, metadataFor: targetOrNull);
            if (targetOrNull != null) l.A("FYI: Set metadata target which was provided.");

            return (newEntity, publishing);
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
        /// <exception cref="ArgumentException">Attribute in attributes does not exit</exception>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        public void Update(int entityId, Dictionary<string, object> values) => Log.Do($"update i:{entityId}", () =>
        {
            var original = AppSysCtx.AppState.List.FindRepoId(entityId);
            var import = BuildNewEntity(original.Type, values, null, original.IsPublished);
            // #ExtractEntitySave - verified
            _appWork.Value.EntityUpdate(null, appState: AppSysCtx.AppState)
                .UpdateParts(entityId, import.Entity as Entity, import.DraftAndBranch);
        });


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


        private IDictionary<string, object> ConvertRelationsToNullArray(IContentType contentType,
            IDictionary<string, object> values) => Log.Func(() =>
        {
            // Find all attributes which are relationships
            var relationships = contentType.Attributes.Where(a => a.Type == ValueTypes.Entity).ToList();

            var newValues = values.ToDictionary(pair => pair.Key, pair =>
            {
                var value = pair.Value;
                // Not relationship, don't convert
                if (!relationships.Any(a => a.Name.EqualsInsensitive(pair.Key)))
                    return value;

                switch (value)
                {
                    case null: return null;
                    case int intVal:
                        return new List<int?> { intVal };
                    case Guid guidVal:
                        return new List<Guid?> {guidVal };
                    case IEnumerable<int> idInt:
                        return idInt.Cast<int?>().ToList();
                    case IEnumerable<int?> idIntNull:
                        return idIntNull.ToList();
                    case IEnumerable<Guid> idGuid:
                        return idGuid.Cast<Guid?>().ToList();
                    case IEnumerable<Guid?> idGuidNull:
                        return idGuidNull.ToList();
                    case string strValEmpty when !strValEmpty.HasValue(): return null;
                    case string strVal:
                        var parts = strVal.Split(',').Where(s => s.HasValue()).ToList();
                        if (parts.Count == 0) return value;

                        // could be int/guid - must convert - must all be the same
                        if (int.TryParse(parts[0], out var intValue))
                            return parts.Select(item => int.TryParse(item, out intValue) ? (int?)intValue : null).ToList();
                        
                        if (Guid.TryParse(parts[0], out var guidValue))
                            return parts.Select(item => Guid.TryParse(item, out guidValue) ? (Guid?)guidValue : null).ToList();

                        // fallback
                        return value;
                    default:
                        return value;
                }
            });
            return newValues;
        });

        private (bool ShouldPublish, bool DraftShouldBranch)? FigureOutPublishing(
            IContentType contentType,
            IDictionary<string, object> values,
            bool? existingIsPublished
        ) => Log.Func($"..., ..., attributes: {values?.Count}", l =>
        {
            (bool ShouldPublish, bool DraftShouldBranch)? publishAndBranch = null;
            if (/*global::ToSic.Eav.Plumbing.Linq.IEnumerableExtensions.SafeAny(values) &&*/ values.SafeNone())
                return (publishAndBranch, "no attributes to process");

            // On update, by default preserve IsPublished state
            var isPublished = existingIsPublished ?? true;

            // Ensure WritePublished or WriteDraft user permissions. 
            var allowed = GetWriteAndPublishAllowed(contentType);
            if (!allowed.WriteAllowed)
                throw new Exception("User is not allowed to do anything. Both published and draft are not allowed.");

            // IsPublished becomes false when write published is not allowed.
            if (isPublished && !allowed.PublishAllowed) isPublished = false;

            // Find publishing instructions
            // Handle special "PublishState" attribute
            var publishKvp = values.FirstOrDefault(pair => pair.Key.EqualsInsensitive(SaveApiAttributes.SavePublishingState));
            if (publishKvp.Key != default)  // must check key, because kvps don't have a null-default
            {
                publishAndBranch = GetPublishSpecs(
                    publishedState: publishKvp.Value,
                    existingIsPublished: isPublished,
                    allowed.PublishAllowed);

                isPublished = publishAndBranch.Value.ShouldPublish;

                l.A($"IsPublished: {isPublished}");
            }
            
            return (publishAndBranch, "done");
        });

        private IDictionary<string, IAttribute> BuildNewEntityValues(
            IContentType contentType,
            IImmutableDictionary<string, IAttribute> attributes,
            string valuesLanguage
        ) => Log.Func($"..., ..., attributes: {attributes?.Count}, {valuesLanguage}", l =>
        {
            if (attributes.SafeNone())
                return (new Dictionary<string, IAttribute>(), "null/empty");

            //var tempMutable = _builder.Attribute.Mutable(attributes);

            var updated = attributes.Select(keyValuePair =>
                {
                    // Handle content-type attributes
                    var ctAttr = contentType[keyValuePair.Key];
                    if (ctAttr != null && keyValuePair.Value != null)
                    {
                        attributes.TryGetValue(ctAttr.Name, out var attribute);
                        var firstValue = keyValuePair.Value.Values?.FirstOrDefault();
                        var firstValContents = firstValue?.ObjectContents;
                        if (firstValContents == null) return null;
                        var preConverted =
                            _builder.Value.PreConvertReferences(firstValContents, ctAttr.Type, true);
                        var newAttribute = _builder.Attribute.CreateOrUpdate(originalOrNull: attribute, name: ctAttr.Name, value: preConverted,
                            type: ctAttr.Type, valueToReplace: firstValue, language: valuesLanguage);
                        l.A($"Attribute '{keyValuePair.Key}' will become '{keyValuePair.Value}' ({ctAttr.Type})");
                        return new
                        {
                            keyValuePair.Key,
                            Attribute = newAttribute
                        };
                    }

                    return null;
                })
                .Where(x => x != null)
                .ToDictionary(pair => pair.Key, pair => pair.Attribute, InvariantCultureIgnoreCase);
            return (updated, "done");
        });

        #region Permission Checks

        private (bool PublishAllowed, bool WriteAllowed) GetWriteAndPublishAllowed(IContentType targetType)
        {
            // skip write publish/draft permission checks for c# API
            if (!_checkWritePermissions) return (true, true);

            // this write publish/draft permission checks should happen only for REST API

            // 1. Find if user may write PUBLISHED:

            // 1.1. app permissions 
            if (_appPermissionCheckGenerator.New().ForAppInInstance(_ctx, AppSysCtx.AppState)
                .UserMay(GrantSets.WritePublished)) return (true, true);

            // 1.2. type permissions
            if (_appPermissionCheckGenerator.New().ForType(_ctx, AppSysCtx.AppState, targetType)
                .UserMay(GrantSets.WritePublished)) return (true, true);


            // 2. Find if user may write DRAFT:

            // 2.1. app permissions 
            if (_appPermissionCheckGenerator.New().ForAppInInstance(_ctx, AppSysCtx.AppState)
                .UserMay(GrantSets.WriteDraft)) return (false, true);

            // 2.2. type permissions
            if (_appPermissionCheckGenerator.New().ForType(_ctx, AppSysCtx.AppState, targetType)
                .UserMay(GrantSets.WriteDraft)) return (false, true);


            // 3. User is not allowed to update published or draft entity.
            return (false, false);
        }


        #endregion
    }
}