using ToSic.Eav.Apps.Inspect.Sys.Models;
using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Inspect.Sys;

/// <summary>
/// The details service is used by some app data sources.
/// </summary>
public interface IAppDetailsService
{
    AppDetailsRaw GetDetails(ISite site, IAppReader appReader);
}