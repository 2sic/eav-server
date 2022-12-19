using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Internal intermediate object when retrieving an Entity property.
    /// Will contain additional information for upstream processing
    /// </summary>
    [PrivateApi]
    public class PropReqResult
    {
        public PropReqResult(object result, PropertyLookupPath path)
        {
            Result = result;
            Path = path;
        }

        /// <summary>
        /// The result of the request - null if not found
        /// </summary>
        public object Result;

        /// <summary>
        /// Debug property to see if a result was wrapped to create something else
        /// </summary>
        internal object ResultOriginal;

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

        public readonly PropertyLookupPath Path;

        public int SourceIndex = -1;

        public bool IsFinal => SourceIndex != -1;

        /// <summary>
        /// Special Helper to return nothing, easier to spot in code when this is used
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static PropReqResult Null(PropertyLookupPath path) => new PropReqResult(null, path);
        public static PropReqResult NullFinal(PropertyLookupPath path) => Null(path).AsFinal(0);

        public PropReqResult AsFinal(int sourceIndex)
        {
            SourceIndex = sourceIndex;
            return this;
        }
    }
}
