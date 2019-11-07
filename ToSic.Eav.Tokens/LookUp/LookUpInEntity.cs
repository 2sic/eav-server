using System;
using System.Linq;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.LookUp
{
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	[PublicApi]
	public class LookUpInEntity : LookUpBase
    {
        protected IEntity Entity;
	    private readonly string[] _dimensions = {""};

        /// <summary>
        /// not sure if I can drop this - or if the empty constructor is needed for DI
        /// </summary>
        [PrivateApi]
	    public LookUpInEntity()
	    {
	        
	    }

	    /// <summary>
	    /// Constructs a new Entity LookUp
	    /// </summary>
	    /// <param name="source"></param>
	    /// <param name="name">Name of the LookUp, e.g. Settings</param>
	    public LookUpInEntity(IEntity source, string name = "entity source without name")
		{
            Entity = source;
		    Name = name;
		}

        // todo: might need to clarify what language/culture the key is taken from in an entity
        /// <summary>
        /// Special lookup command with format-provider.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <param name="notFound"></param>
        /// <returns></returns>
        private string Get(string key, string format, System.Globalization.CultureInfo formatProvider, ref bool notFound)
        {
            // Return empty string if Entity is null
            if (Entity == null)
            {
                notFound = true;
                return string.Empty;
            }

            // bool notFound;
            var valueObject = Entity.GetBestValue(key, _dimensions);
            notFound = valueObject == null; // this is used multiple times

            if (!notFound)
            {
                switch (Type.GetTypeCode(valueObject.GetType()))
                {
                    case TypeCode.String:
                        return FormatString((string)valueObject, format);
                    case TypeCode.Boolean:
                        return ((bool)valueObject).ToString(formatProvider).ToLower();
                    case TypeCode.DateTime:
                        if (string.IsNullOrEmpty(format))
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
                                .Get(subTokens.Rest, format, formatProvider, ref notFound);

                    #endregion
                }
                    
            }
            #endregion

            notFound = true;
            return string.Empty;
        }

        /// <inheritdoc/>
        public override string Get(string key, string format, ref bool notFound)
	        => Get(key, format, System.Threading.Thread.CurrentThread.CurrentCulture, ref notFound);

        /// <inheritdoc/>
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