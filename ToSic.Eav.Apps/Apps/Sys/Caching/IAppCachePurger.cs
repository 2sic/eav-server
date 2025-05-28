using ToSic.Eav.Apps;

namespace ToSic.Eav.Caching;

/// <summary>
/// Service which picks the current Apps Cache to use.
/// </summary>
/// <remarks>
/// Also used in the database layer, so it cannot be in the Apps project.
/// </remarks>
public interface IAppCachePurger
{
    /// <summary>
    /// Clean cache for specific Zone and App
    /// </summary>
    void Purge(IAppIdentity app);
}