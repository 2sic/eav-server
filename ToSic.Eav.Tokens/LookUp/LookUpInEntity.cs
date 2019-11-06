using System;
using System.Linq;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.LookUp
{
	/// <inheritdoc />
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class LookUpInEntity : LookUpBase
    {
        protected IEntity Entity;
	    private readonly string[] _dimensions = {""};

	    public LookUpInEntity()
	    {
	        
	    }

	    /// <summary>
	    /// Constructs a new AssignedEntity AttributePropertyAccess
	    /// </summary>
	    /// <param name="source"></param>
	    /// <param name="name">Name of the PropertyAccess, e.g. pipelinesettings</param>
	    public LookUpInEntity(IEntity source, string name = "entity source without name")
		{
            Entity = source;
		    Name = name;
		}

        // todo: might need to clarify what language/culture the key is taken from in an entity
        public string Get(string key, string format, System.Globalization.CultureInfo formatProvider, ref bool propertyNotFound)
        {
            // Return empty string if Entity is null
            if (Entity == null)
            {
                propertyNotFound = true;
                return string.Empty;
            }

            // bool propertyNotFound;
            var valueObject = Entity.GetBestValue(key, _dimensions);
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
                        if (String.IsNullOrEmpty(format))
                        {
                            // make sure datetime is converted as universal time with the correct format specifier if no format is given
                            return ((DateTime)valueObject).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        return ((DateTime)valueObject).ToString(format, formatProvider);
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                        var outputFormat = format == string.Empty ? "g" : format;
                        return ((IFormattable)valueObject).ToString(outputFormat, formatProvider);
                    default:
                        return FormatString(valueObject.ToString(), format);
                }
            }

            #region Not found yet, so check for Navigation-Property (e.g. Manager:Name)

            var subTokens = CheckAndGetSubToken(key);
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
                            : new LookUpInEntity(relationshipList.First())
                                .Get(subTokens.Rest, format, formatProvider, ref propertyNotFound);

                    #endregion
                }
                    
            }
            #endregion

            propertyNotFound = true;
            return string.Empty;
        }


	    public override string Get(string key, string format, ref bool propertyNotFound)
	        => Get(key, format, System.Threading.Thread.CurrentThread.CurrentCulture, ref propertyNotFound);
        

	    public override bool Has(string key)
	    {
	        var notFound = !Entity?.Attributes.ContainsKey(key) ?? false; // always false if no entity attached
            // if it's not a standard attribute, check for dynamically provided values like EntityId
            if (notFound)
	            Get(key, "", ref notFound);
	        return !notFound;

	    }
    }
}