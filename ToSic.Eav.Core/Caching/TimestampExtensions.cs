﻿namespace ToSic.Eav.Caching;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class TimestampExtensions
{
    public static DateTime ToDateTime(this long original) => new(original);

    public static string ToReadable(this long original) => original.ToDateTime().ToString("s");
}