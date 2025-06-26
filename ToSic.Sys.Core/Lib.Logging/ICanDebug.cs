namespace ToSic.Lib.Logging;

/// <summary>
/// Trivial interface just to ensure that we have debug on/off consistent
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICanDebug
{
    /// <summary>
    /// Debug state on/off
    /// </summary>
    bool Debug { get; set; }
}