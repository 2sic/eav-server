using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc cref="IValue{T}" />
    /// <summary>
    /// Represents a Value
    /// </summary>
    /// <typeparam name="T">Type of the actual Value</typeparam>
    public class Value<T> : Value, IValue<T>
    {
        public T TypedContents { get; internal set; }

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

        public Value(T typedContents)
        {
            TypedContents = typedContents;
        }

        public object ObjectContents => TypedContents;
        public IValue Copy(string type) => ValueBuilder.Build(type, ObjectContents,
            Languages.Select(l => new Dimension {DimensionId = l.DimensionId, Key = l.Key} as ILanguage).ToList(), null);
    }
}