namespace ToSic.Lib.Logging;

/// <summary>
/// Special, rarely used options to optimize logging
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntryOptions
{
    public bool HideCodeReference { get; set; }
    public bool ShowNewLines { get; set; }
}