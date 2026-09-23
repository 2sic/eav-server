using ToSic.Eav.Context;

namespace ToSic.Eav.WebApi.Sys.App.Details;

/// <summary>
/// The details service is used by some app data sources.
/// </summary>
public interface IAppDetailsService
{
    AppDetailsRaw GetDetails(ISite site, IAppReader appReader);
}