using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ValueTypeHelpers
{
    /// <summary>
    /// Look up the type code if we started with a string.
    /// This is case-insensitive.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static ValueTypes Get(string typeName) 
        => Enum.TryParse<ValueTypes>(typeName ?? "", true, out var code) ? code : ValueTypes.Undefined;
}