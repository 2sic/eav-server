using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Internal intermediate object when retrieving an Entity property.
    /// Will contain additional information for upstream processing
    /// </summary>
    [PrivateApi]
    public class PropertyRequest
    {
        /// <summary>
        /// The result of the request - null if not found
        /// </summary>
        public object Result;


        /// <summary>
        /// The IValue object, in case we need to use it's cache
        /// </summary>
        public IValue Value;
        
        /// <summary>
        /// A field type, like "Hyperlink" or "Entity" etc.
        /// </summary>
        public string FieldType;
        
        /// <summary>
        /// The entity which returned this property
        /// </summary>
        public object Source;

        /// <summary>
        /// An optional name
        /// </summary>
        public string Name;

        public PropertyLookupPath Path;

        public int SourceIndex = -1;

        public bool IsFinal => SourceIndex != -1;

        public PropertyRequest AsFinal(int sourceIndex)
        {
            SourceIndex = sourceIndex;
            return this;
        }
    }
}
