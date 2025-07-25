﻿// Keep this in the EAV.Identity namespace,
// Since we use it in some of our projects and that would be breaking otherwise.
// In future, we should provide this on a service and then hide this...

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Identity;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class Mapper
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string GuidCompress(this Guid newGuid)
    {
        var modifiedBase64 = Convert.ToBase64String(newGuid.ToByteArray())
            .Replace('+', '-').Replace('/', '_')    // avoid invalid URL characters
            .Substring(0, 22);                      // truncate trailing "==" characters
        return modifiedBase64;
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static Guid GuidRestore(string shortGuid)
    {
        var base64 = shortGuid.Replace('-', '+').Replace('_', '/') + "==";
        var bytes = Convert.FromBase64String(base64);
        return new(bytes);
    }

}