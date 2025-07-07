﻿namespace ToSic.Sys.Capabilities.Features;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class FeatureSecurity(int impact, string message = "")
{
    public int Impact { get; } = impact;

    public string Message { get; } = message;

    /// <summary>
    /// For fallback in null-cases, probably not used ATM
    /// </summary>
    public static FeatureSecurity Unknown = new(0, SysConstants.Unknown);
}