namespace ToSic.Sys.Logging;

/// <summary>
/// Obsolete logging-connection extension retained as a no-op for compatibility.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
// ReSharper disable once InconsistentNaming
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class IHasLogExtensions
{
    /// <summary>
    /// Obsolete no-op. Returns the original object so existing command chains continue to compile.
    /// </summary>
    /// <param name="thingWithLog">Object which is an IHasLog</param>
    /// <param name="parentLog">Former parent log; ignored.</param>
    /// <param name="forceConnect">Former override; ignored.</param>
    /// <returns>The same object as started this, to allow chaining</returns>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static T LinkLog<T>(this T thingWithLog, ILog? parentLog, bool forceConnect = false) where T: class, IHasLog
        => thingWithLog;

}
