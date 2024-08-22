using ToSic.Eav.Apps.State;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Apps;

/// <summary>
/// Internal system to retrieve AppReaders for accessing app state/data directly.
///
/// It is documented but not meant to be used outside the core team. 
/// </summary>
/// <remarks>
/// This was introduced in 2sxc 18 to replace older AppStates mechanisms.
/// It ensures that the reading actions are done in a controlled way, and that the app state is always up-to-date.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IAppReaderFactory
{
    /// <summary>
    /// Helper for internal purposes. Sometimes code will get an app identity,
    /// but it could also be a Reader. For performance reasons, this will get you the reader, but
    /// possibly without having to create a new one.
    /// </summary>
    /// <param name="appOrReader"></param>
    /// <returns></returns>
    IAppReader GetOrKeep(IAppIdentity appOrReader);

    /// <summary>
    /// Get a reader for the specified app.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    IAppReader Get(IAppIdentity app);

    /// <summary>
    /// Get a reader for the specified app.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    IAppReader Get(int appId);

    /// <summary>
    /// Get a reader of the zone's primary app.
    /// Typically, this is he site-primary app, which contains site metadata and settings.
    /// </summary>
    /// <param name="zoneId"></param>
    /// <returns></returns>
    IAppReader GetZonePrimary(int zoneId);

    /// <summary>
    /// Get the preset App of the system.
    /// In a very special case, it should skip this if it's not loaded.
    /// </summary>
    /// <param name="protector"></param>
    /// <param name="nullIfNotLoaded"></param>
    /// <returns></returns>
    IAppReader GetSystemPreset(NoParamOrder protector = default, bool nullIfNotLoaded = false);


    //IAppReader ToReader(IAppStateCache state);
}