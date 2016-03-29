using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport.Refactoring.Extensions;

namespace ToSic.Eav.Api.Api01
{
    /// <summary>
    /// This is a simple controller with some Create, Update and Delete commands. 
    /// </summary>
    public class SimpleDataController
    {
        private readonly EavDataController Context;

        private readonly string _defaultLanguageCode;

        private readonly int _zoneId;

        private readonly int _appId;

        private readonly string _userName;



        /// <summary>
        /// Create a simple data controller to create, update and delete entities.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="appId">App ID</param>
        /// <param name="userName">Name of user loged in</param>
        /// <param name="defaultLanguageCode">Default language of system</param>
        public SimpleDataController(int zoneId, int appId, string userName, string defaultLanguageCode)
        {
            _zoneId = zoneId;
            _appId = appId;
            _userName = userName;
            _defaultLanguageCode = defaultLanguageCode;
            Context = EavDataController.Instance(zoneId, appId);
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
        public void Create(string contentTypeName, Dictionary<string, object> values, bool filterUnknownFields = true)
        {
            var attributeSet = Context.AttribSet.GetAllAttributeSets().FirstOrDefault(item => item.Name == contentTypeName);
            if (attributeSet == null)
            {
                throw new ArgumentException("Content type '" + contentTypeName + "' does not exist.");
            }

            if (filterUnknownFields)
                values = RemoveUnknownFields(values, attributeSet);

            var importEntity = CreateImportEntity(attributeSet.StaticName);
            importEntity.AppendAttributeValues(attributeSet, ConvertEntityRelations(values), _defaultLanguageCode, false, true);
            importEntity.Import(_zoneId, _appId, _userName);
        }

        private static Dictionary<string, object> RemoveUnknownFields(Dictionary<string, object> values, AttributeSet attributeSet)
        {
            var listAllowed = attributeSet.GetAttributes();
            values =
                values.Where(x => listAllowed.Any(y => y.StaticName == x.Key))
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
        public void Update(int entityId, Dictionary<string, object> values, bool filterUnknownFields = true)
        {
            var entity = Context.Entities.GetEntity(entityId);
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
        public void Update(Guid entityGuid, Dictionary<string, object> values, bool filterUnknownFields = true)
        {
            var entity = Context.Entities.GetEntity(entityGuid);
            Update(entity, values);
        }

        private void Update(Entity entity, Dictionary<string, object> values, bool filterUnknownFields = true)
        {
            var attributeSet = Context.AttribSet.GetAttributeSet(entity.AttributeSetID);
            var importEntity = CreateImportEntity(entity.EntityGUID, attributeSet.StaticName);

            if (filterUnknownFields)
                values = RemoveUnknownFields(values, attributeSet);

            importEntity.AppendAttributeValues(attributeSet, ConvertEntityRelations(values), _defaultLanguageCode, false, true);
            importEntity.Import(_zoneId, _appId, _userName);
        }


        /// <summary>
        /// Delete the entity specified by ID.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <exception cref="InvalidOperationException">Entity cannot be deleted for example when it is referenced by another object</exception>
        public void Delete(int entityId)
        {
            // todo: refactor to use the eav-api delete
            if (!Context.Entities.CanDeleteEntity(entityId)/*_contentContext.EntCommands.CanDeleteEntity(entityId)*/.Item1)
            {
                throw new InvalidOperationException("The entity " + entityId + " cannot be deleted because of it is referenced by another object.");
            }
            Context.Entities.DeleteEntity(entityId);
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
            var entity = Context.Entities.GetEntity(entityGuid);
            Delete(entity.EntityID);
        }



        private static ImportEntity CreateImportEntity(string attributeSetStaticName)
        {
            return CreateImportEntity(Guid.NewGuid(), attributeSetStaticName);
        }

        private static ImportEntity CreateImportEntity(Guid entityGuid, string attributeSetStaticName)
        {
            return new ImportEntity
            {
                EntityGuid = entityGuid,
                AttributeSetStaticName = attributeSetStaticName,
                AssignmentObjectTypeId = Configuration.AssignmentObjectTypeIdDefault, // SexyContent.AssignmentObjectTypeIDDefault,
                KeyNumber = null,
                Values = new Dictionary<string, List<IValueImportModel>>()
            };
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
                        var entity = Context.Entities.GetEntity(id);
                        guids.Add(entity.EntityGUID);
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
    }
}