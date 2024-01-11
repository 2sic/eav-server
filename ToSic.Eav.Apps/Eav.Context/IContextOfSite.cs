using ToSic.Lib.Logging;

namespace ToSic.Eav.Context;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IContextOfSite: IHasLog, IContextOfUserPermissions
{
    /// <summary>
    /// The website the current request is running in
    /// </summary>
    ISite Site { get; set; }

    /// <summary>
    /// The user in the current request / context
    /// </summary>
    IUser User { get; }

    /// <summary>
    /// Create a clone of the context, usually for then making a slightly different context
    /// </summary>
    /// <returns></returns>
    IContextOfSite Clone(ILog newParentLog);
}