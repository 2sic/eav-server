﻿namespace ToSic.Lib.Logging;

/// <summary>
/// Special, rarely used options to optimize logging
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntryOptions
{
    public bool HideCodeReference { get; set; }
    public bool ShowNewLines { get; set; }
}