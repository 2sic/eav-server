﻿namespace ToSic.Eav.Identity;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class Mapper
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string GuidCompress(this Guid newGuid)
    {
        var modifiedBase64 = Convert.ToBase64String(newGuid.ToByteArray())
            .Replace('+', '-').Replace('/', '_')    // avoid invalid URL characters
            .Substring(0, 22);                      // truncate trailing "==" characters
        return modifiedBase64;
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Guid GuidRestore(string shortGuid)
    {
        var base64 = shortGuid.Replace('-', '+').Replace('_', '/') + "==";
        var bytes = Convert.FromBase64String(base64);
        return new(bytes);
    }

}