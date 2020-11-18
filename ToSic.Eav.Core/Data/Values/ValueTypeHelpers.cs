using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    internal class ValueTypeHelpers
    {
        public static ValueTypes Get(string typeName)
        {
            // if the type has not been set yet, try to look it up...
            if (typeName != null && Enum.IsDefined(typeof(ValueTypes), typeName))
                return (ValueTypes)Enum.Parse(typeof(ValueTypes), typeName);
            return ValueTypes.Undefined;
        }
    }
}
