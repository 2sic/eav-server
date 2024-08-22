using ToSic.Eav.Apps.State;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Apps;

public interface IAppReaders
{
    IAppReader GetOrKeep(IAppIdentity appOrReader);

    IAppReader Get(IAppIdentity app);

    IAppReader Get(int appId);

    IAppReader GetZonePrimary(int zoneId);

    /// <summary>
    /// Get the preset App of the system.
    /// In a very special case, it should skip this if it's not loaded.
    /// </summary>
    /// <param name="protector"></param>
    /// <param name="nullIfNotLoaded"></param>
    /// <returns></returns>
    IAppReader GetSystemPreset(NoParamOrder protector = default, bool nullIfNotLoaded = false);

    IAppReader ToReader(IAppStateCache state);
}