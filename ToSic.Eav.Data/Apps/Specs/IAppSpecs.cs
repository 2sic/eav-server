using ToSic.Eav.Apps.State;
using ToSic.Eav.Metadata;
using ToSic.Lib.Data;

namespace ToSic.Eav.Apps.Internal.Specs;

/// <summary>
/// This interface says this object knows all the specs of an App.
/// It's primarily used so helper functions can tell us more about the App
/// by receiving a TODO object which has this hidden somewhere.
/// </summary>
public interface IAppSpecs : IAppIdentity, IHasIdentityNameId
{
    string Name { get; }

    string Folder { get; }

    IAppConfiguration Configuration { get; }

    /// <summary>
    /// Metadata describing the current App.
    /// </summary>
    IMetadataOf Metadata { get; }

    IAppStateMetadata Settings { get; }

    IAppStateMetadata Resources { get; }

}
