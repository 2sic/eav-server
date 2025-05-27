using System.Collections.Specialized;

namespace ToSic.Lib.LookUp.Sources;

/// <summary>
/// Look-Up helper to get something from a standard .net NameValueCollection. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("ATM not in use.")]
public class LookUpInNameValueCollection(string name, NameValueCollection list)
    : LookUpBase(name, $"Lookup in {nameof(NameValueCollection)} object.")
{
    /// <inheritdoc />
    public override string Get(string key, string format) 
        => list == null!
            ? "" 
            : FormatString(list[key] ?? "", format);
}