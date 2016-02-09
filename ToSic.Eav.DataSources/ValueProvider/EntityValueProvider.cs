using System;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.ValueProvider
{
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class EntityValueProvider : BaseValueProvider
    {
        protected IEntity _entity;
	    private string[] dimensions = new string[] {""};

	    public EntityValueProvider()
	    {
	        
	    }

		/// <summary>
		/// Constructs a new AssignedEntity AttributePropertyAccess
		/// </summary>
		/// <param name="name">Name of the PropertyAccess, e.g. pipelinesettings</param>
		/// <param name="objectId">EntityGuid of the Entity to get assigned Entities of</param>
		/// <param name="metaDataSource">DataSource that provides MetaData</param>
        public EntityValueProvider(IEntity source, string name = "entity source without name")
		{
            _entity = source;
		    Name = name;
		}

        // todo: might need to clarify what language/culture the property is taken from in an entity
        public string Get(string property, string format, System.Globalization.CultureInfo formatProvider, ref bool propertyNotFound)
        {
            // Return empty string if Entity is null
            if (_entity == null)
                return string.Empty;

            string outputFormat = format == string.Empty ? "g" : format;

            // bool propertyNotFound;
            object valueObject = _entity.GetBestValue(property, dimensions);//, out propertyNotFound);
            propertyNotFound = (valueObject == null);

            if (!propertyNotFound && valueObject != null)
            {
                switch (Type.GetTypeCode(valueObject.GetType()))// (valueObject.GetType().Name)
                {
                    case TypeCode.String: // "String": 
                        return FormatString((string)valueObject, format);
                    case TypeCode.Boolean:// "Boolean": 
                        return ((bool)valueObject).ToString(formatProvider).ToLower();
                    case TypeCode.DateTime:// "DateTime": 
                    case TypeCode.Double: // "Double":
                    case TypeCode.Single: // "Single":
                    case TypeCode.Int16:  //
                    case TypeCode.Int32: // "Int32":
                    case TypeCode.Int64: // "Int64":
                    case TypeCode.Decimal: // "Decimal":
                        return (((IFormattable)valueObject).ToString(outputFormat, formatProvider));
                    default:
                        return FormatString(valueObject.ToString(), format);
                }
            }
            else
            {
                #region Check for Navigation-Property (e.g. Manager:Name)

                var subTokens = CheckAndGetSubToken(property);
                if (subTokens.HasSubtoken)
                {
                    valueObject = _entity.GetBestValue(subTokens.Source, dimensions);

                    if (valueObject != null)
                    {
                        #region Handle child-Entity-Field (sorted list of related entities)
                        var relationshipList = valueObject as Data.EntityRelationship;
                        if (relationshipList != null)
                        {
                            if (!relationshipList.Any())
                                return string.Empty;
                            else
                                return new EntityValueProvider(relationshipList.First())
                                    .Get(subTokens.Rest, format, formatProvider, ref propertyNotFound);
                        }
                        #endregion
                    }
                    
                }
                #endregion

                propertyNotFound = true;
                return string.Empty;
            }
        }


        public override string Get(string property, string format, ref bool PropertyNotFound)
        {
            return Get(property, format, System.Threading.Thread.CurrentThread.CurrentCulture, ref PropertyNotFound);
        }

	    public override bool Has(string property)
	    {
	        var notFound = !_entity.Attributes.ContainsKey(property);
            // if it's not a standard attribute, check for dynamically provided values like EntityId
            if (notFound)
	            Get(property, "", ref notFound);
	        return !notFound;

	    }
    }
}