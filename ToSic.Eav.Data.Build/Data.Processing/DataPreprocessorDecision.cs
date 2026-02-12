namespace ToSic.Eav.Data.Processing;

public enum DataPreprocessorDecision
{
    Continue = 0,
    Skip = 50,

    /// <summary>
    /// Throw an exception - has a very high number, so if we add more variants it doesn't shift the values around.
    /// </summary>
    Error = 100,
}
