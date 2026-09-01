namespace ToSic.Sys.DI;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILazyInitLog
{
    /// <summary>
    /// Initializer to attach the log to the generator.
    /// The log is later given to generated objects.
    /// </summary>
    /// <param name="parentLog">Parent Log</param>
    void SetLog(ILog? parentLog);
}