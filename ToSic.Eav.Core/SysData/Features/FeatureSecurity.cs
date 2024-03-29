﻿namespace ToSic.Eav.SysData;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FeatureSecurity(int impact, string message = "")
{
    public int Impact { get; } = impact;

    public string Message { get; } = message;

    /// <summary>
    /// For fallback in null-cases, probably not used ATM
    /// </summary>
    public static FeatureSecurity Unknown = new(0, Constants.NullNameId);
}