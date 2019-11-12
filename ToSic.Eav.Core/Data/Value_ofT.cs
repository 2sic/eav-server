using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a typed Value object in the memory model
    /// </summary>
    /// <typeparam name="T">Type of the actual Value</typeparam>
    [PublicApi]
    public class Value<T> : Value, IValue<T>
    {
        /// <inheritdoc />
        public T TypedContents { get; internal set; }

        /// <inheritdoc />
        public object SerializableObject
        {
            get
            {
                var typedObject = ((IValue<T>)this).TypedContents;

                // special case with list of related entities - should return array of guids
                var maybeRelationshipList = typedObject as EntityRelationship;
                if (maybeRelationshipList != null)
                {
                    var entityGuids = maybeRelationshipList.Select(e => e?.EntityGuid);
                    return entityGuids.ToList();
                }

                return typedObject;
            }
        }

        [PrivateApi("used only for xml-serialization, does very specific date-to-string conversions")]
        public string Serialized
        {
            get
            {
                var obj = SerializableObject;
                if (obj is List<Guid?> list)
                    return string.Join(",", list.Select(y => y?.ToString() ?? Constants.EmptyRelationship));

                return (obj as DateTime?)?.ToString("yyyy-MM-ddTHH:mm:ss") 
                    ?? (obj as bool?)?.ToString() 
                    ?? (obj as decimal?)?.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    ?? obj?.ToString();
            }
        }

        /// <summary>
        /// The default constructor to create a value object. Used internally to build the memory model. 
        /// </summary>
        /// <param name="typedContents"></param>
        public Value(T typedContents)
        {
            TypedContents = typedContents;
        }

        [PrivateApi]
        public object ObjectContents => TypedContents;

        [PrivateApi]
        public IValue Copy(string type) => ValueBuilder.Build(type, ObjectContents,
            Languages.Select(l => new Dimension {DimensionId = l.DimensionId, Key = l.Key} as ILanguage).ToList(), null);
    }
}