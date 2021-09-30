using ToSic.Eav.Documentation;

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



        #region Obsolete stuff, but still included
        // 2021-09-30 2dm completely turned this off, believe that it's been marked obsolete for a year, shouldn't be used anywhere external

        ///// <summary>
        ///// Get Attribute for specified Typ
        ///// </summary>
        ///// <returns><see cref="Attribute{ValueType}"/></returns>
        //[PrivateApi("moved to some attribute-builder or something - was always marked as PrivatAPI, so we should be able to kill soon")]
        //[Obsolete("moved to AttributeBuilder.CreateTyped, but will soon be a non-static implementation")]
        //public static IAttribute CreateTypedAttribute(string name, ValueTypes type, List<IValue> values = null) 
        //    => AttributeBuilder.CreateTyped(name, type, values);

        //[PrivateApi]
        //[Obsolete("moved to AttributeBuilder")]
        //public static IAttribute CreateTypedAttribute(string name, string type, List<IValue> values = null)
        //    => CreateTypedAttribute(name, ValueTypeHelpers.Get(type), values);

        #endregion
    }
}