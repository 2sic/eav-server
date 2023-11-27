using System;

namespace ToSic.Lib.Documentation;

/// <summary>
/// This attribute adds special documentation to the code, which is still available in the DLL.
/// Experimental, currently just used to add more info to MetadataTargetTypes
/// </summary>
// TODO: TEMPORARY name DocWIP because there is another Docs attribute - should probably be merged
[PrivateApi("WIP / Experimental")]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class DocsWip : Attribute
{
    public string Documentation { get; }

    /// <summary>
    /// Constructor with required comment `[Docs(some-comment)]`
    /// </summary>
    /// <param name="docs">Reason why it's public, optional</param>
    public DocsWip(string docs)
    {
        Documentation = docs;
    }
}