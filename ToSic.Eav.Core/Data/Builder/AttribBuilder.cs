using System;
using System.Collections.Generic;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Builder
{
    public class AttribBuilder
    {

        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        public static Dictionary<string, IAttribute> ConvertToInvariantDic(IDictionary<string, object> objAttributes)
        {
            var result = new Dictionary<string, IAttribute>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var oAttrib in objAttributes)
            {
                // in case the object is already an IAttribute, use that, don't rebuild it
                if (oAttrib.Value is IAttribute typedValue)
                    result[oAttrib.Key] = typedValue;
                else
                {
                    var attributeType = DataTypes.GetAttributeTypeName(oAttrib.Value);
                    var attributeModel = AttributeBuilder.CreateTyped(oAttrib.Key, attributeType);
                    var valuesModelList = new List<IValue>();
                    if (oAttrib.Value != null)
                    {
                        var valueModel = new ValueBuilder().Build(attributeType, oAttrib.Value, null);
                        valuesModelList.Add(valueModel);
                    }

                    attributeModel.Values = valuesModelList;

                    result[oAttrib.Key] = attributeModel;
                }
            }

            return result;
        }
    }
}
