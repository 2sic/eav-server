using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repository.Efc;
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
    public class SimpleDataController: HasLog
    {
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly DbDataController _dbData;
        public Lazy<AttributeBuilder> LazyAttributeBuilder { get; }
        private DbDataController _context;

        private AppManager _appManager;

        private string _defaultLanguageCode;

        private int _appId;

        #region Constructor / DI

        /// <summary>
        /// Create a simple data controller to create, update and delete entities.
        /// Used for DI - must always call Init afterwards
        /// </summary>
        public SimpleDataController(Lazy<AttributeBuilder> lazyAttributeBuilder, Lazy<AppManager> appManagerLazy, DbDataController dbData): base("Dta.Simple")
        {
            _appManagerLazy = appManagerLazy;
            _dbData = dbData;
            LazyAttributeBuilder = lazyAttributeBuilder;
        }

        /// <param name="zoneId">Zone ID</param>
        /// <param name="appId">App ID</param>
        /// <param name="defaultLanguageCode">Default language of system</param>
        /// <param name="parentLog"></param>
        internal SimpleDataController Init(int zoneId, int appId, string defaultLanguageCode, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _appId = appId;
            _defaultLanguageCode = defaultLanguageCode;
            _context = _dbData.Init(zoneId, appId, Log);
            _appManager = _appManagerLazy.Value.Init(new AppIdentity(zoneId, appId), Log);
            return this;
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
            var wrapLog = Log.Call<IEnumerable<int>>(contentTypeName);

            // ensure the type really exists
            var type = _appManager.Read.ContentTypes.Get(contentTypeName);
            if (type == null)
            {
                var msg = "Error: Content type '" + contentTypeName + "' does not exist.";
                wrapLog(msg, null);
                throw new ArgumentException(msg);
            }

            var importEntity = multiValues.Select(values => BuildEntity(type, values, target)).ToList();

            var ids = _appManager.Entities.Save(importEntity);
            return wrapLog(null, ids);
        }

        private IEntity BuildEntity(IContentType type, Dictionary<string, object> values, ITarget target)
        {
            var wrapLog = Log.Call<IEntity>();
            // ensure it's case insensitive...
            values = new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);

            if (values.All(v => v.Key.ToLowerInvariant() != Constants.EntityFieldGuid))
            {
                Log.Add("add guid");
                values.Add(Constants.EntityFieldGuid, Guid.NewGuid());
            }

            var eGuid = Guid.Parse(values[Constants.EntityFieldGuid].ToString());
            var importEntity = new Entity(_appId, eGuid, type, new Dictionary<string, object>());
            if (target != null)
            {
                Log.Add("add metadata target");
                importEntity.SetMetadata(target);
            }
            AppendAttributeValues(importEntity, type, ConvertEntityRelations(values), _defaultLanguageCode, false,
                true);
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
            _appManager.Entities.UpdateParts(entityId, values);
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
                else
                    result.Add(value.Key, value.Value);
            return result;
        }

        private void AppendAttributeValues(Entity entity, IContentType attributeSet, Dictionary<string, object> values, string valuesLanguage, bool valuesReadOnly, bool resolveHyperlink)
        {
            Log.Add("append attribute values");
            foreach (var value in values)
            {
                // Handle special attributes (for example of the system)
                if (value.Key.ToLowerInvariant() == Constants.EntityFieldIsPublished)
                {
                    entity.IsPublished = value.Value as bool? ?? true;
                    Log.Add($"isPublished: {entity.IsPublished}");
                    continue;
                }

                // Ignore entity guid - it's already set earlier
                if (value.Key.ToLowerInvariant() == Constants.EntityFieldGuid)
                {
                    Log.Add("entity-guid, ignore here");
                    continue;
                }

                // Handle content-type attributes
                var attribute = attributeSet[value.Key];
                if (attribute != null)
                    LazyAttributeBuilder.Value.AddValue(entity.Attributes, 
                    /*entity.Attributes.AddValue(*/attribute.Name, value.Value.ToString(), attribute.Type, valuesLanguage, valuesReadOnly, resolveHyperlink);
            }
        }
    }
}