using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Parts;

// This is the simple API used to quickly create/edit/delete entities
// It's in the Apps-project, because we are trying to elliminate the plain ToSic.Eav as it was structured in 2016

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Api.Api01
{
    /// <summary>
    /// This is a simple controller with some Create, Update and Delete commands. 
    /// </summary>
    public class SimpleDataController
    {
        private readonly DbDataController _context;

        private readonly string _defaultLanguageCode;

        private readonly int _zoneId;

        private readonly int _appId;

        /// <summary>
        /// Create a simple data controller to create, update and delete entities.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="appId">App ID</param>
        ///// <param name="userName">Name of user loged in</param>
        /// <param name="defaultLanguageCode">Default language of system</param>
        public SimpleDataController(int zoneId, int appId, string defaultLanguageCode)
        {
            _zoneId = zoneId;
            _appId = appId;
            _defaultLanguageCode = defaultLanguageCode;
            _context = DbDataController.Instance(zoneId, appId);
        }



        /// <summary>
        /// Create a new entity of the content-type specified.
        /// </summary>
        /// <param name="contentTypeName">Content-type</param>
        /// <param name="values">
        ///     Values to be set collected in a dictionary. Each dictionary item is a pair of attribute 
        ///     name and value. To set references to other entities, set the attribute value to a list of 
        ///     entity ids. 
        /// </param>
        /// <exception cref="ArgumentException">Content-type does not exist, or an attribute in values</exception>
        public void Create(string contentTypeName, Dictionary<string, object> values)//, bool filterUnknownFields = true)
        {
            // ensure it's case insensitive...
            values = new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);

            // ensure the type really exists
            var attributeSet = _context.AttribSet.GetAllAttributeSets().FirstOrDefault(item => item.Name == contentTypeName);
            if (attributeSet == null)
                throw new ArgumentException("Content type '" + contentTypeName + "' does not exist.");

            if(values.All(v => v.Key.ToLower() != Constants.EntityFieldGuid))
                values.Add(Constants.EntityFieldGuid, Guid.NewGuid());

            var eGuid = Guid.Parse(values[Constants.EntityFieldGuid].ToString());
            var importEntity = new ImpEntity(eGuid, attributeSet.StaticName, new Dictionary<string, object>());// CreateImportEntity(eGuid, attributeSet.StaticName);
            AppendAttributeValues(importEntity, attributeSet, ConvertEntityRelations(values), _defaultLanguageCode, false, true);
            ExecuteImport(importEntity);
        }

        private static Dictionary<string, object> RemoveUnknownFields(Dictionary<string, object> values, ToSicEavAttributeSets attributeSet)
        {
            // todo: ensure things like IsPublished and EntityGuid don't get filtered...
            // part of https://github.com/2sic/2sxc/issues/1173
            var listAllowed = attributeSet.GetAttributes();
            var allowedNames = listAllowed.Select(a => a.StaticName.ToLower()).ToList();
            allowedNames.Add(Constants.EntityFieldGuid);
            allowedNames.Add(Constants.EntityFieldIsPublished);
            values = values.Where(x => allowedNames.Any(y => y == x.Key.ToLower()))
                .ToDictionary(x => x.Key, y => y.Value);
            return values;
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
            var entity = _context.Entities.GetDbEntity(entityId);
            Update(entity, values);
        }

        /// <summary>
        /// Update an entity specified by GUID.
        /// </summary>
        /// <param name="entityGuid">Entity GUID</param>param>
        /// <param name="values">
        ///     Values to be set collected in a dictionary. Each dictionary item is a pair of attribute 
        ///     name and value. To set references to other entities, set the attribute value to a list of 
        ///     entity ids. 
        /// </param>
        /// <exception cref="ArgumentException">Attribute in values does not exit</exception>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        public void Update(Guid entityGuid, Dictionary<string, object> values)//, bool filterUnknownFields = true)
        {
            var entity = _context.Entities.GetMostCurrentDbEntity(entityGuid);
            Update(entity, values);
        }

        private void Update(ToSicEavEntities entity, Dictionary<string, object> values, bool filterUnknownFields = true)
        {
            var attributeSet = _context.AttribSet.GetAttributeSet(entity.AttributeSetId);
            var importEntity = new ImpEntity(entity.EntityGuid, attributeSet.StaticName, new Dictionary<string, object>());// CreateImportEntity(entity.EntityGuid, attributeSet.StaticName);

            if (filterUnknownFields)
                values = RemoveUnknownFields(values, attributeSet);

            AppendAttributeValues(importEntity, attributeSet, ConvertEntityRelations(values), _defaultLanguageCode, false, true);
            ExecuteImport(importEntity);
        }


        /// <summary>
        /// Delete the entity specified by ID.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(int entityId)
        {
            // todo: refactor to use the eav-api delete
            if (!_context.Entities.CanDeleteEntity(entityId).Item1)
                throw new InvalidOperationException("The entity " + entityId +
                                                    " cannot be deleted because of it is referenced by another object.");
            _context.Entities.DeleteEntity(entityId);
        }


        /// <summary>
        /// Delete the entity specified by GUID.
        /// </summary>
        /// <param name="entityGuid">Entity GUID</param>
        /// <exception cref="ArgumentNullException">Entity does not exist</exception>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(Guid entityGuid)
        {
            // todo: refactor to use the eav-api delete
            var entity = _context.Entities.GetMostCurrentDbEntity(entityGuid);
            Delete(entity.EntityId);
        }


        private Dictionary<string, object> ConvertEntityRelations(Dictionary<string, object> values)
        {
            var result = new Dictionary<string, object>();
            foreach (var value in values)
            {
                var ids = value.Value as IEnumerable<int>;
                if (ids != null)
                {   // The value has entity ids. For import, these must be converted to a string of guids.
                    var guids = new List<Guid>();
                    foreach (var id in ids)
                    {
                        var entity = _context.Entities.GetDbEntity(id);
                        guids.Add(entity.EntityGuid);
                    }
                    result.Add(value.Key, string.Join(",", guids));
                }
                else
                {
                    result.Add(value.Key, value.Value);
                }
            }
            return result;
        }

        private void ExecuteImport(ImpEntity impEntity)
        {
            var import = new DbImport(_zoneId, _appId, false);
            import.ImportIntoDb(null, new[] { impEntity });
            SystemManager.Purge(_zoneId, _appId);
        }

        private void AppendAttributeValues(ImpEntity impEntity, ToSicEavAttributeSets attributeSet, Dictionary<string, object> values, string valuesLanguage, bool valuesReadOnly, bool resolveHyperlink)
        {
            foreach (var value in values)
            {
                // Handle special attributes (for example of the system)
                if (value.Key.ToLower() == Constants.EntityFieldIsPublished)
                {
                    impEntity.IsPublished = value.Value as bool? ?? true;
                    continue;
                }

                // Ignore entity guid - it's already set earlier
                if (value.Key.ToLower() == Constants.EntityFieldGuid)
                    continue;

                // Handle content-type attributes
                var attribute = attributeSet.AttributeByName(value.Key);
                if (attribute != null)
                    impEntity.Attributes.AddValue(attribute.StaticName, value.Value.ToString(), attribute.Type, valuesLanguage, valuesReadOnly, resolveHyperlink);
            }
        }
    }
}