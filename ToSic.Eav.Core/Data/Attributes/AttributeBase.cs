using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents an Attribute Definition. Used in Content-Types and IEntities.
    /// </summary>
    /// <remarks>
    /// > We recommend you read about the [](xref:Basics.Data.Index)
    /// </remarks>
    [PrivateApi("Hidden in 12.04 2021-09 because people should only use the interface - previously InternalApi. This is just fyi, use Interface IAttributeBase")]
    public class AttributeBase : IAttributeBase
    {
        /// <inheritdoc />
        public string Name { get; set; }
        /// <inheritdoc />
        public string Type { get; set; }

        private ValueTypes _controlledType = ValueTypes.Undefined;

        [PrivateApi]
        public ValueTypes ControlledType
        {
            get => _controlledType != ValueTypes.Undefined 
                ? _controlledType
                : _controlledType = ValueTypeHelpers.Get(Type);
            internal set => _controlledType = value;
        }

        /// <summary>
        /// Extended constructor when also storing the persistence ID-Info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        [PrivateApi]
        public AttributeBase(string name, string type)
        {
            Name = name;
            Type = type;
		}
    }
}