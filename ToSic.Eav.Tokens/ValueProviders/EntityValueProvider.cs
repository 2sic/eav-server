using System;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ValueProviders
{
	/// <inheritdoc />
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class EntityValueProvider : BaseValueProvider
    {
        protected IEntity Entity;
	    private readonly string[] _dimensions = {""};

	    public EntityValueProvider()
	    {
	        
	    }

	    /// <summary>
	    /// Constructs a new AssignedEntity AttributePropertyAccess
	    /// </summary>
	    /// <param name="source"></param>
	    /// <param name="name">Name of the PropertyAccess, e.g. pipelinesettings</param>
	    public EntityValueProvider(IEntity source, string name = "entity source without name")
		{
            Entity = source;
		    Name = name;
		}

        // todo: might need to clarify what language/culture the property is taken from in an entity
        public string Get(string property, string format, System.Globalization.CultureInfo formatProvider, ref bool propertyNotFound)
        {
            // Return empty string if Entity is null
            if (Entity == null)
            {
                propertyNotFound = true;
                return string.Empty;
            }

            var outputFormat = format == string.Empty ? "g" : format;

            // bool propertyNotFound;
            var valueObject = Entity.GetBestValue(property, _dimensions);
            propertyNotFound = valueObject == null; // this is used multiple times

            if (!propertyNotFound)
            {
                switch (Type.GetTypeCode(valueObject.GetType()))
                {
                    case TypeCode.String:
                        return FormatString((string)valueObject, format);
                    case TypeCode.Boolean:
                        return ((bool)valueObject).ToString(formatProvider).ToLower();
                    case TypeCode.DateTime:
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                        return ((IFormattable)valueObject).ToString(outputFormat, formatProvider);
                    default:
                        return FormatString(valueObject.ToString(), format);
                }
            }

            #region Not found yet, so check for Navigation-Property (e.g. Manager:Name)

            var subTokens = CheckAndGetSubToken(property);
            if (subTokens.HasSubtoken)
            {
                valueObject = Entity.GetBestValue(subTokens.Source, _dimensions);

                if (valueObject != null)
                {
                    #region Handle child-Entity-Field (sorted list of related entities)
                    var relationshipList = valueObject as Data.EntityRelationship;
                    if (relationshipList != null)
                        return !relationshipList.Any()
                            ? string.Empty
                            : new EntityValueProvider(relationshipList.First())
                                .Get(subTokens.Rest, format, formatProvider, ref propertyNotFound);

                    #endregion
                }
                    
            }
            #endregion

            propertyNotFound = true;
            return string.Empty;
        }


	    public override string Get(string property, string format, ref bool propertyNotFound)
	        => Get(property, format, System.Threading.Thread.CurrentThread.CurrentCulture, ref propertyNotFound);
        

	    public override bool Has(string property)
	    {
	        var notFound = !Entity?.Attributes.ContainsKey(property) ?? false; // always false if no entity attached
            // if it's not a standard attribute, check for dynamically provided values like EntityId
            if (notFound)
	            Get(property, "", ref notFound);
	        return !notFound;

	    }
    }
}