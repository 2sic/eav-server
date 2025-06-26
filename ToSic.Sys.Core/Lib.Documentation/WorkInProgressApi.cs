namespace ToSic.Lib.Documentation;

/// <summary>
/// This attribute marks APIs to be publicly documented with a clear warning that it's work in progress.
/// </summary>
/// <remarks>
/// Constructor. WIP attributes must always have a comment why they are WIP.
/// </remarks>
/// <param name="comment">Reason why it's WIP, required</param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[ShowApiWhenReleased(ShowApiMode.Never)]
#pragma warning disable CS9113 // Parameter is unread.
public class WorkInProgressApi(string comment) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.
