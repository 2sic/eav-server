namespace ToSic.Sys.Documentation;

/// <summary>
/// This attribute marks APIs to be publicly documented with a clear warning that it's **deprecated**.
/// </summary>
/// <remarks>
/// Introduced in v22.
/// </remarks>
/// <param name="comment">Reason why it's deprecated, required</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
#pragma warning disable CS9113 // Parameter is unread.
public class DeprecatedApi(string comment, string started, string expectedRemoval) : Attribute;
