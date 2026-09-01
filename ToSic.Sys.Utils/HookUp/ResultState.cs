namespace ToSic.Sys.HookUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public enum ResultState
{
    /// <summary>
    /// Nothing special to do, just continue.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Skip this data. For example, when the pre-processor already handled it in some custom way.
    /// </summary>
    Skip = 50,

    
    /// <summary>
    /// Stop further processing in a sequence, signal that further steps must be skipped.
    /// </summary>
    StopSequence = 100,

    /// <summary>
    /// Throw an exception - has a very high number, so if we add more variants it doesn't shift the values around.
    /// </summary>
    Error = 10000,
}
