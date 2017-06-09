using System;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Note: seems to be a helper class with tools 
    /// todo: probably refactor to fit into some "normal" object 
    /// </summary>
    public class AttributeHelperTools
    {
        //private static readonly Value<EntityRelationship> EntityRelationshipDefaultValue = new Value<EntityRelationship>(new EntityRelationship(null)) { Languages = new Dimension[0] };

        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        public static Dictionary<string, IAttribute> GetTypedDictionaryForSingleLanguage(IDictionary<string, object> attributes, string titleAttributeName)
        {
            var result = new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase);

            foreach (var attribute in attributes)
            {
                var attributeType = GetAttributeTypeName(attribute.Value);
                var baseModel = new AttributeBase(attribute.Key, attributeType, attribute.Key == titleAttributeName, attributeId: 0, sortOrder: 0);
                var attributeModel = GetAttributeManagementModel(baseModel);
                var valuesModelList = new List<IValue>();
                if (attribute.Value != null)
                {
                    var valueModel = Value.Build(baseModel.Type, attribute.Value.ToString(), null, null);
                    valuesModelList.Add(valueModel);
                }

                attributeModel.Values = valuesModelList;

                result[attribute.Key] = attributeModel;
            }

            return result;
        }

        /// <summary>
        /// Get EAV AttributeType for a value, like String, Number, DateTime or Boolean
        /// </summary>
        static string GetAttributeTypeName(object value)
        {
            if (value is DateTime)
                return "DateTime";
            if (value is decimal || value is int || value is double)
                return "Number";
            if (value is bool)
                return "Boolean";
            return "String";
        }

        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
		/// <returns><see cref="Attribute{ValueType}"/></returns>
        public static IAttributeManagement GetAttributeManagementModel(IAttributeBase definition)
        {
            switch (definition.ControlledType)
            {
                case AttributeTypeEnum.Boolean:  
                    return new Attribute<bool?>(definition.Name, definition.Type, definition.IsTitle, definition.AttributeId, definition.SortOrder);
                case AttributeTypeEnum.DateTime:
                    return new Attribute<DateTime?>(definition.Name, definition.Type, definition.IsTitle, definition.AttributeId, definition.SortOrder);
                case AttributeTypeEnum.Number:
                    return new Attribute<decimal?>(definition.Name, definition.Type, definition.IsTitle, definition.AttributeId, definition.SortOrder);
                case AttributeTypeEnum.Entity: 
                    return new Attribute<EntityRelationship>(definition.Name, definition.Type, definition.IsTitle, definition.AttributeId, definition.SortOrder) { Values = new IValue[] { Value.NullRelationship /*EntityRelationshipDefaultValue */} };
                case AttributeTypeEnum.String:
                case AttributeTypeEnum.Hyperlink:
                case AttributeTypeEnum.Custom:
                default:
                    return new Attribute<string>(definition.Name, definition.Type, definition.IsTitle, definition.AttributeId, definition.SortOrder);
            }
        }
    }
}