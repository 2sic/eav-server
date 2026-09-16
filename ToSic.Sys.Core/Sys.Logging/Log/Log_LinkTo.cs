namespace ToSic.Sys.Logging;

public partial class Log
{
    /// <summary>
    /// Link this logger to a parent
    /// and optionally rename
    /// </summary>
    /// <param name="newParent">parent log to attach to</param>
    /// <param name="name">optional new name</param>
    internal void LinkTo(ILog? newParent, string? name = default)
    {
        var explicitParentOperation = (newParent as ILogCall)?.Entry;
        newParent = newParent.GetRealLog();

        if (newParent == this)
            throw new("LOG ERROR - attaching a log to itself can't work");

        // A deliberately supplied call remains an explicit operation token without retaining a log parent.
        AttachmentOperation = explicitParentOperation;
        if (name != null)
            this.Rename(name);
    }

    // 2025-03-26 2dm commented out, seems to be unused
    ///// <summary>
    ///// Would unlink this - but unclear if ever to be used
    ///// </summary>
    //public void Unlink() => Parent = null;

}
