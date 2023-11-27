using System;

namespace ToSic.Lib.Documentation;

/// <summary>
/// This attribute marks APIs to be publicly documented with a clear warning that it's work in progress.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkInProgressApi : Attribute
{
    /// <summary>
    /// Constructor. WIP attributes must always have a comment why they are WIP.
    /// </summary>
    /// <param name="comment">Reason why it's WIP, required</param>
    public WorkInProgressApi(string comment) { }
}