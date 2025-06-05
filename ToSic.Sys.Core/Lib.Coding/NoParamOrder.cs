namespace ToSic.Lib.Coding;

/// <summary>
/// Special placeholder to indicate that all parameters following this should be named.
/// </summary>
/// <remarks>
/// It is really important that all parameters following this are named, since the API won't guarantee the order of the parameter names.
///
/// Example: This signature:
///
/// `Something.Save(string name, NoParamOrder noParamOrder = default, string title = title, string description = default)`
/// 
/// allows:
/// 
/// 1. `Something.Save("MyName", title: "MyTitle")`
/// 2. `Something.Save("MyName", description: "MyDescription")`
/// 3. `Something.Save("MyName")` - without additional parameters
/// 
/// See [Convention: Named Parameters](https://go.2sxc.org/named-params).
/// </remarks>
[PublicApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public struct NoParamOrder
{
    public const string HelpLink = "https://go.2sxc.org/named-params";
}
