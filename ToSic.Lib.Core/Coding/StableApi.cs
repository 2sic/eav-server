using ToSic.Lib.Documentation;

namespace ToSic.Lib.Coding;

/// <summary>
/// Special placeholder to indicate that all parameters following this should be named.
/// It is really important that all parameters following this are named, since the API won't guarantee the order of the parameter names.
///
/// Example:
///
/// * This signature: `...Save(string name, NoParamOrder noParamOrder = default, string title = title, string description = default)`
/// * allows a) `Save("MyName", title: "MyTitle")` and b) `Save("MyName", description: "MyDescription")`
/// * or c) Save("MyName") - without additional parameters
/// See [Convention: Named Parameters(https://go.2sxc.org/named-params).
/// </summary>
[PublicApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public struct NoParamOrder
{
}
