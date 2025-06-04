using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Apps;

/// <summary>
/// This is the internal, official way to access data from an App.
/// * In certain cases it will do other things, such as retrieve more data from elsewhere to be sure that everything is available.
/// * It is for short term use only, so don't cache this object
///
/// To get an app Reader, use the ??? TODO
/// </summary>
public interface IAppReader:
    IAppIdentity,
    IAppReadEntities,
    IAppReadContentTypes
{
    /// <summary>
    /// Various App Specs such as IDs, folders and more.
    /// </summary>
    IAppSpecs Specs { get; }

    /// <summary>
    /// Metadata Source to get Metadata of anything in the App.
    /// </summary>
    IMetadataSource Metadata { get; }

    /// <summary>
    /// WIP 2025-03-27 2dm / STV
    /// </summary>
    bool IsHealthy { get; }

    /// <summary>
    /// WIP 2025-03-27 2dm / STV
    /// </summary>
    string HealthMessage { get; }

    /// <summary>
    /// internal, but in some cases it will be given out by hidden extension methods
    /// </summary>
    internal IAppStateCache AppState { get; }
}