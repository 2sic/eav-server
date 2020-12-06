using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ToSic.Eav.Context;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Get Values from an <see cref="IEntity"/>. <br/>
    /// Read more about this in @Specs.LookUp.Intro
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	public class LookUpInEntity : LookUpIn<IEntity>
    {
        private readonly string[] _dimensions;

	 //   /// <summary>
	 //   /// Constructs a new Entity LookUp
	 //   /// </summary>
	 //   /// <param name="source"></param>
	 //   /// <param name="name">Name of the LookUp, e.g. Settings</param>
	 //   [Obsolete("You should use the constructor providing language information")]
	 //   public LookUpInEntity(string name, IEntity source): this(name, source, null)
  //      {
		//}

        /// <summary>
        /// Constructs a new Entity LookUp
        /// </summary>
        /// <param name="name">Name of the LookUp, e.g. Settings</param>
        /// <param name="source"></param>
        /// <param name="dimensions">the languages / dimensions to use</param>
        public LookUpInEntity(string name, IEntity source, string[] dimensions): base(source, name)
        {
            _dimensions = dimensions ?? IZoneCultureResolverExtensions.SafeCurrentDimensions(null);
        }

        // todo: might need to clarify what language/culture the key is taken from in an entity
        /// <summary>
        /// Special lookup command with format-provider.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public override string Get(string key, string format)
        {
            // Return empty string if Entity is null
            if (Data == null)
                return string.Empty;

            // bool notFound;
            var valueObject = Data.GetBestValue(key, _dimensions);
            var notFound = valueObject == null; // this is used multiple times

            if (!notFound)
            {
                switch (Type.GetTypeCode(valueObject.GetType()))
                {
                    case TypeCode.String:
                        return FormatString((string)valueObject, format);
                    case TypeCode.Boolean:
                        return Format((bool) valueObject);
                    case TypeCode.DateTime:
                        // make sure datetime is converted as universal time with the correct format specifier if no format is given
                        return !string.IsNullOrWhiteSpace(format)
                            ? ((DateTime) valueObject).ToString(format, IZoneCultureResolverExtensions.SafeCurrentCultureInfo(_dimensions))
                            : Format((DateTime) valueObject);
                        //((DateTime) valueObject).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                        // make sure it's converted to a neutral number format with "." notation if no format was given
                        return !string.IsNullOrWhiteSpace(format)
                            ? ((IFormattable) valueObject).ToString(format, IZoneCultureResolverExtensions.SafeCurrentCultureInfo(_dimensions))
                            : ((IFormattable) valueObject).ToString("G", CultureInfo.InvariantCulture);
                        //var outputFormat = string.IsNullOrWhiteSpace(format) ? "g" : format;
                        //return ((IFormattable)valueObject).ToString(outputFormat, formatProvider);
                    default:
                        return FormatString(valueObject.ToString(), format);
                }
            }

            // Not found yet, so check for Navigation-Property (e.g. Manager:Name)
            var subTokens = CheckAndGetSubToken(key);
            if (!subTokens.HasSubtoken) return string.Empty;
            valueObject = Data.GetBestValue(subTokens.Source, _dimensions);
            if (valueObject == null) return string.Empty;

            // Finally: Handle child-Entity-Field (sorted list of related entities)
            if (!(valueObject is IEnumerable<IEntity> relationshipList)) return string.Empty;
            var first = relationshipList.FirstOrDefault();
            return first == null
                ? string.Empty
                : new LookUpInEntity("no-name", first, _dimensions).Get(subTokens.Rest, format);
        }
    }
}