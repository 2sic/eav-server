namespace ToSic.Eav.Context;

/// <summary>
/// Service to provide the context of an app.
/// </summary>
public interface IContextOfAppService : IContextOfSiteService
{
    /// <summary>
    /// Retrieve the current app with reader etc. or null if not available.
    /// </summary>
    /// <remarks>
    /// Can be standalone (App only) or with a block (App + Block).
    /// </remarks>
    IContextOfApp? AppContextOrNull { get; }
}
