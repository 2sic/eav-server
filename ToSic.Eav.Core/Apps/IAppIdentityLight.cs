﻿using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Marks things which belongs to an App - but it may only know about the app, not about the zone. For a full identity, see <see cref="IAppIdentity"/>.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IAppIdentityLight
    {
        /// <summary>
        /// The app id as used internally
        /// </summary>
        /// <returns>The App ID this thing belongs to</returns>
        int AppId { get; }
    }

    // ReSharper disable once InconsistentNaming
    public static class IAppIdentityExtensions
    {
        public static string Show(this IAppIdentity appId) => $"{appId.ZoneId}/{appId.AppId}";
    }
}
