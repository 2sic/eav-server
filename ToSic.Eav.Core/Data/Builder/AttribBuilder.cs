using System;
using System.Collections.Generic;

namespace ToSic.Eav.Data.Builder
{
    public class AttribBuilder
    {

        public AttribBuilder(ValueBuilder valueBuilder)
        {
            _valueBuilder = valueBuilder;
        }
        private readonly ValueBuilder _valueBuilder;

        protected AttributeBuilder AttributeBuilder => _attributeBuilder ?? (_attributeBuilder = new AttributeBuilder(_valueBuilder));
        private AttributeBuilder _attributeBuilder;

        public static AttribBuilder GetStatic() => new AttribBuilder(new ValueBuilder(new DimensionBuilder()));

        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        public Dictionary<string, IAttribute> ConvertToInvariantDic(IDictionary<string, object> objAttributes)
        {
            var result = new Dictionary<string, IAttribute>(StringComparer.InvariantCultureIgnoreCase);

            // Process each property
            foreach (var oAttrib in objAttributes)
            {
                // in case the object is already an IAttribute, use that, don't rebuild it
                if (oAttrib.Value is IAttribute typedValue)
                    result[oAttrib.Key] = typedValue;
                else
                {
                    var attributeType = DataTypes.GetAttributeTypeName(oAttrib.Value);
                    var valuesModelList = new List<IValue>();
                    if (oAttrib.Value != null)
                    {
                        var valueModel = _valueBuilder.Build(attributeType, oAttrib.Value);
                        valuesModelList.Add(valueModel);
                    }
                    var attributeModel = AttributeBuilder.CreateTyped(oAttrib.Key, attributeType, valuesModelList);

                    // #immutable
                    //attributeModel.Values = valuesModelList;

                    result[oAttrib.Key] = attributeModel;
                }
            }

            return result;
        }
    }
}
