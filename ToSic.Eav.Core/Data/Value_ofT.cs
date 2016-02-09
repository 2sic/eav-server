using System;
using System.Linq;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    /// <typeparam name="T">Type of the actual Value</typeparam>
    public class Value<T> : Value, IValue<T>, IValueManagement
    {
        public T TypedContents { get; internal set; }

        public string Serialized
        {
            get
            {
                // todo: I moved this from somewhere else - but this class knows what <T> is...
                // ...so I should refactor it to not do all this casting, but just check what type T is

                var value = this;
                var stringValue = value as Value<string>;
                if (stringValue != null)
                    return stringValue.TypedContents;

                var relationshipValue = value as Value<EntityRelationship>;
                if (relationshipValue != null)
                {
                    var entityGuids = relationshipValue.TypedContents.Select(e => e?.EntityGuid.ToString() ?? "null");                    //var entityGuids = relationshipValue.TypedContents.EntityIds.Select(entityId => entityId.HasValue ? Context.Entities.GetEntity(entityId.Value).EntityGUID : Guid.Empty);
                    return string.Join(",", entityGuids);
                }

                var boolValue = value as Value<bool?>;
                if (boolValue != null)
                    return boolValue.TypedContents.ToString();

                var dateTimeValue = value as Value<DateTime?>;
                if (dateTimeValue != null)
                    return dateTimeValue.TypedContents.HasValue ? dateTimeValue.TypedContents.Value.ToString("s") : "";

                var decimalValue = value as Value<decimal?>;
                if (decimalValue != null)
                    return decimalValue.TypedContents.HasValue ? decimalValue.TypedContents.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";

                throw new NotSupportedException("Can't serialize Value");
            }
        }

        public object SerializableObject
        {
            get
            {
                var typedObject = ((IValue<T>)this).TypedContents;

                // special case with list of related entities - should return array of guids
                var maybeRelationshipList = typedObject as EntityRelationship;
                if (maybeRelationshipList != null)
                {
                    var entityGuids = maybeRelationshipList.Select(e => e?.EntityGuid);                    //var entityGuids = relationshipValue.TypedContents.EntityIds.Select(entityId => entityId.HasValue ? Context.Entities.GetEntity(entityId.Value).EntityGUID : Guid.Empty);
                    return entityGuids.ToList();
                }

                return typedObject;
            }
        }

        public Value(T typedContents)
        {
            TypedContents = typedContents;
        }

    }
}