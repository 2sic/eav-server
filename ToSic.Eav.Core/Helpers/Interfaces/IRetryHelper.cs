namespace ToSic.Eav.Helpers.Interfaces;

public interface IRetryHelper
{
    /// <summary>
    /// Executes an action with retry logic. In case a transient SQL error or timeout occurs,
    /// the operation will be retried using exponential backoff with jitter.
    /// </summary>
    /// <param name="action">The code to execute that might need to be retried.</param>
    /// <param name="maxRetries">Maximum number of retries. Default is 3.</param>
    /// <param name="initialDelayMilliseconds">Initial delay before the first retry. Default is 500 ms.</param>
    /// <param name="maxDelayMilliseconds">Maximum delay between retries. Default is 30000 ms.</param>
    void ExecuteWithRetry(Action action, int maxRetries = 3, int initialDelayMilliseconds = 500, int maxDelayMilliseconds = 30000);

    /// <inheritdoc />
    ILog Log { get; }
}