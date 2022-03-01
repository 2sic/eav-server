using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Api.Api01;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Run;
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
    public partial class SimpleDataController: HasLog<SimpleDataController>
    {
        #region Constructor / DI

        /// <summary>
        /// Used for DI - must always call Init to use
        /// </summary>
        public SimpleDataController(Lazy<AttributeBuilder> lazyAttributeBuilder, Lazy<AppManager> appManagerLazy, Lazy<DbDataController> dbDataLazy, IZoneMapper zoneMapper, ISite site): base("Dta.Simple")
        {
            _appManagerLazy = appManagerLazy;
            _dbDataLazy = dbDataLazy;
            _zoneMapper = zoneMapper.Init(Log);
            _site = site;
            _lazyAttributeBuilder = lazyAttributeBuilder;
        }
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly Lazy<DbDataController> _dbDataLazy;
        private readonly IZoneMapper _zoneMapper;
        private readonly ISite _site;

        public AttributeBuilder AttributeBuilder => _attributeBuilder ?? (_attributeBuilder = _lazyAttributeBuilder.Value.Init(Log));
        private AttributeBuilder _attributeBuilder;
        private readonly Lazy<AttributeBuilder> _lazyAttributeBuilder;
        private DbDataController _context;
        private AppManager _appManager;
        private string _defaultLanguageCode;

        private int _appId;

        /// <param name="zoneId">Zone ID</param>
        /// <param name="appId">App ID</param>
        public SimpleDataController Init(int zoneId, int appId)
        {
            var wrapLog = Log.Call<SimpleDataController>($"{zoneId}, {appId}");
            _appId = appId;
            _defaultLanguageCode = GetDefaultLanguage();
            _context = _dbDataLazy.Value.Init(zoneId, appId, Log);
            _appManager = _appManagerLazy.Value.Init(new AppIdentity(zoneId, appId), Log);
            Log.Add($"Default language:{_defaultLanguageCode}");
            return wrapLog(null, this);
        }

        private string GetDefaultLanguage()
        {
            var usesLanguages = _zoneMapper.CulturesWithState(_site).Any(c => c.IsEnabled);
            return usesLanguages ? _site.DefaultCultureCode : "";
        }

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
        public IEnumerable<int> Create(string contentTypeName, IEnumerable<Dictionary<string, object>> multiValues, ITarget target = null)
        {
            var wrapLog = Log.Call<IEnumerable<int>>($"{contentTypeName}, items: {multiValues?.Count()}, target: {target != null}");

            if (multiValues == null) return wrapLog("values were null", null);

            // ensure the type really exists
            var type = _appManager.Read.ContentTypes.Get(contentTypeName);
            if (type == null)
            {
                var msg = "Error: Content type '" + contentTypeName + "' does not exist.";
                wrapLog(msg, null);
                throw new ArgumentException(msg);
            }

            Log.Add($"Type {contentTypeName} found. Will build entities to save...");

            var importEntity = multiValues.Select(values => BuildEntity(type, values, target)).ToList();

            var ids = _appManager.Entities.Save(importEntity);
            return wrapLog(null, ids);
        }

        private IEntity BuildEntity(IContentType type, Dictionary<string, object> values, ITarget target)
        {
            var wrapLog = Log.Call<IEntity>($"{type.Name}, {values?.Count}, target: {target != null}");
            // ensure it's case insensitive...
            values = new Dictionary<string, object>(values, StringComparer.InvariantCultureIgnoreCase);

            if (values.All(v => v.Key.ToLowerInvariant() != Attributes.EntityFieldGuid))
            {
                Log.Add("Add new generated guid, as none was provided.");
                values.Add(Attributes.EntityFieldGuid, Guid.NewGuid());
            }

            // Get owner form value dictionary (and remove it from values) because we need to provided it in entity constructor.
            string owner = null;
            if (values.Any(v => v.Key.ToLowerInvariant() == Attributes.EntityFieldOwner))
            {
                Log.Add("Get owner, when is provided.");
                owner = values[Attributes.EntityFieldOwner].ToString();
                values.Remove(Attributes.EntityFieldOwner);
            }

            var eGuid = Guid.Parse(values[Attributes.EntityFieldGuid].ToString());
            var importEntity = new Entity(_appId, eGuid, type, new Dictionary<string, object>(), owner);
            if (target != null)
            {
                Log.Add("Set metadata target which was provided.");
                importEntity.SetMetadata(target);
            }

            var preparedValues = ConvertEntityRelations(values);
            AddValues(importEntity, type, preparedValues, _defaultLanguageCode, false, true);
            return wrapLog(null, importEntity);
        }


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
            Log.Add($"update i:{entityId}");
            var original = _appManager.AppState.List.FindRepoId(entityId);

            bool? draft = null;
            if (values.Keys.Contains(SaveApiAttributes.SavePublishingState))
            {
                draft = IsDraft(values, original);
                Log.Add($"contains IsPublished value d:{draft}");
            }
            else
                values.Add(SaveApiAttributes.SavePublishingState, original.IsPublished); // original "IsPublished" initial state, temp store in "values" (so it is forwarded in BuildEntity AddValues where it will be removed)

            var importEntity = BuildEntity(original.Type, values, null) as Entity;

            _appManager.Entities.UpdateParts(entityId, importEntity, draft);
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
            Log.Add("convert entity relations");
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

        private void AddValues(Entity entity, IContentType contentType, Dictionary<string, object> valuePairs, string valuesLanguage, bool valuesReadOnly, bool resolveHyperlink)
        {
            var wrapLog = Log.Call($"..., ..., values: {valuePairs?.Count}, {valuesLanguage}, read-only: {valuesReadOnly}, {nameof(resolveHyperlink)}: {resolveHyperlink}");
            if (valuePairs == null)
            {
                wrapLog("no values");
                return;
            }
            foreach (var keyValuePair in valuePairs)
            {
                // Handle special attributes (for example of the system)
                if (SaveApiAttributes.SavePublishingState.Equals(keyValuePair.Key, InvariantCultureIgnoreCase))
                {
                    switch (keyValuePair.Value)
                    {
                        // TODO: W/@STV - WHAT IF IT'S FALSE?
                        case string draftString when draftString.Equals(SaveApiAttributes.PublishModeDraft, InvariantCultureIgnoreCase): // if IsPublished = "draft"
                            entity.IsPublished = false; // on new: behave as if IsPublished = false
                            break;
                        case bool newValue:
                            entity.IsPublished = newValue;
                            break;
                    }
                    Log.Add($"IsPublished: {entity.IsPublished}");
                    continue;
                }

                // Ignore entity guid - it's already set earlier
                if (keyValuePair.Key.ToLowerInvariant() == Attributes.EntityFieldGuid)
                {
                    Log.Add("entity-guid, ignore here");
                    continue;
                }

                // Handle content-type attributes
                var attribute = contentType[keyValuePair.Key];
                if (attribute != null && keyValuePair.Value != null)
                {
                    var unWrappedValue = UnWrapJValue(keyValuePair.Value);
                    AttributeBuilder.AddValue(entity.Attributes, attribute.Name, unWrappedValue, attribute.Type, valuesLanguage, valuesReadOnly, resolveHyperlink);
                    Log.Add($"Attribute '{keyValuePair.Key}' will become '{unWrappedValue}' ({attribute.Type})");
                }
            }

            wrapLog("done");
        }

        private static object UnWrapJValue(object original)
        {
            // it is not clear why we are doing type conversion to string (so it will stay like that)
            // but string conversion is causing issue with DateTime (2sxc#2658) so we should not convert DateTime to string
            if (original is JValue jValue && jValue.Type is JTokenType.Date) // JTokenType.Date
                return jValue.Value as DateTime?;

            // If it's not System.DateTime, ToString() it
            // Note 2022-02-11 2dm/STV we believe this is a forced unwrap from the JValue
            // And maybe this would also work with JValue.Value or something
            // But if we change this, it must be tested well, so no priority
            if (!(original is DateTime)) return original.ToString();
            // System.DateTime
            return original;
        }
    }
}