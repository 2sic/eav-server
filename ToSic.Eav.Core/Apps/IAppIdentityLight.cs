using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps;

/// <summary>
/// Marks things which belongs to an App - but it may only know about the app, not about the zone. For a full identity, see <see cref="IAppIdentity"/>.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppIdentityLight
{
    /// <summary>
    /// The app id as used internally
    /// </summary>
    /// <returns>The App ID this thing belongs to</returns>
    int AppId { get; }
}

// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class IAppIdentityExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string Show(this IAppIdentity appId) => $"{appId.ZoneId}/{appId.AppId}";
}