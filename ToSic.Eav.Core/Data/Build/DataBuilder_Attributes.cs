using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Builder;

namespace ToSic.Eav.Data
{
    public partial class DataBuilder
    {
        // 2022-02-13 2dm - disabled, shouldn't be in API any more - wasn't used internally

        ///// <summary>
        ///// Create a new attribute for adding to an Entity.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        ///// <param name="typeName">optional type name as string, like "String" or "Entity" - note that type OR typeName must be provided</param>
        ///// <param name="type">optional type code - note that type OR typeName must be provided</param>
        ///// <param name="values">list of values to add to this attribute</param>
        ///// <returns></returns>
        //public IAttribute Attribute(string name,
        //    string noParamOrder = Parameters.Protector,
        //    string typeName = null,
        //    ValueTypes type = ValueTypes.Undefined, 
        //    IList<IValue> values = null)
        //{
        //    // Make sure that we know what type to create/add
        //    if (type == ValueTypes.Undefined)
        //    {
        //        if (string.IsNullOrWhiteSpace(typeName))
        //            throw new Exception("Argument type or typeName must be provided");

        //        // try to find it using the name - will still be Undefined if not found
        //        type = ValueTypeHelpers.Get(typeName);
        //        if (type == ValueTypes.Undefined)
        //            throw new Exception($"Tried to find '{typeName}' but got {ValueTypes.Undefined}");
        //    }
            
            
        //    return AttributeBuilder.CreateTyped(name, type, values);
        //}
    }
}
