namespace ToSic.Eav.Data.Processing;

public enum DataPreprocessorDecision
{
    /// <summary>
    /// Nothing special to do, just continue.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// Skip this data. For example, when the pre-processor already handled it in some custom way.
    /// </summary>
    Skip = 50,

    /// <summary>
    /// Throw an exception - has a very high number, so if we add more variants it doesn't shift the values around.
    /// </summary>
    Error = 100,
}
