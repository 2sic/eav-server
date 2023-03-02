using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public class ValueTypeHelpers
    {
        /// <summary>
        /// Look up the type code if we started with a string.
        /// This is case-insensitive.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static ValueTypes Get(string typeName) 
            => Enum.TryParse<ValueTypes>(typeName ?? "", true, out var code) ? code : ValueTypes.Undefined;
    }
}
