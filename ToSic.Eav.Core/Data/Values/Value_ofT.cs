using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Build;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a typed Value object in the memory model
    /// </summary>
    /// <typeparam name="T">Type of the actual Value</typeparam>
    [PrivateApi("this is just fyi, always work with interface IValue<T>")]
    public class Value<T> : IValue<T>
    {
        /// <summary>
        /// The default constructor to create a value object. Used internally to build the memory model. 
        /// </summary>
        /// <remarks>
        /// * completely #immutable since v15.04
        /// </remarks>
        internal Value(T typedContents, IImmutableList<ILanguage> languages = null)
        {
            TypedContents = typedContents;
            _languages = languages ?? DimensionBuilder.NoLanguages;
        }

        public T TypedContents { get; }


        /// <inheritdoc />
        public IEnumerable<ILanguage> Languages => _languages;
        private readonly IImmutableList<ILanguage> _languages;

        /// <inheritdoc />
        public object SerializableObject
        {
            get
            {
                var typedObject = TypedContents;

                if (!(typedObject is IEnumerable<IEntity> maybeRelationshipList)) return typedObject;

                // special case with list of related entities - should return array of guids
                var entityGuids = maybeRelationshipList.Select(e => e?.EntityGuid);
                return entityGuids.ToList();
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

        [PrivateApi]
        public IValue Clone(IImmutableList<ILanguage> newLanguages) => new Value<T>(TypedContents, newLanguages);

        [PrivateApi]
        public object ObjectContents => TypedContents;
    }
}