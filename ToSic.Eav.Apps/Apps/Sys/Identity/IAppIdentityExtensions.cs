﻿namespace ToSic.Eav.Apps.Sys;

// ReSharper disable once InconsistentNaming
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class IAppIdentityExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string Show(this IAppIdentity? appIdentity)
        => $"{appIdentity?.ZoneId}/{appIdentity?.AppId}";

    /// <summary>
    /// Take only the identity from a richer object which may have too much other stuff.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IAppIdentityPure PureIdentity(this IAppIdentity appId)
        => new AppIdentityPure(appId.ZoneId, appId.AppId);
}