using System.Threading;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using ToSic.Eav.Helpers.Interfaces;

namespace ToSic.Eav.Persistence.Efc.Helpers;

public class RetryHelper() : ServiceBase("Sql.Rtr"), IRetryHelper
{
    // A shared instance of Random for calculating jitter.
    private static readonly Random Random = new();

    /// <summary>
    /// Executes an action with retry logic. In case a transient SQL error or timeout occurs,
    /// the operation will be retried using exponential backoff with jitter.
    /// </summary>
    /// <param name="action">The code to execute that might need to be retried.</param>
    /// <param name="maxRetries">Maximum number of retries. Default is 3.</param>
    /// <param name="initialDelayMilliseconds">Initial delay before the first retry. Default is 500 ms.</param>
    /// <param name="maxDelayMilliseconds">Maximum delay between retries. Default is 30000 ms.</param>
    public void ExecuteWithRetry(Action action, int maxRetries = 3, int initialDelayMilliseconds = 500, int maxDelayMilliseconds = 30000)
    {
        var l = Log.Fn($"maxRetries:{maxRetries},initialDelayMilliseconds:{initialDelayMilliseconds},maxDelayMilliseconds:{maxDelayMilliseconds}", timer: true);

        var attempt = 0;
        while (true)
        {
            try
            {
                l.A($"Attempt:{attempt + 1}/{maxRetries}.");
                action();
                l.Done("Success: exit the loop");
                break; // Success: exit the loop.
            }
            catch (SqlException ex) when (IsTransient(ex))
            {
                l.Ex(ex);

                attempt++;
                if (attempt > maxRetries)
                {
                    l.E($"Maximum retry attempts exceeded.");
                    throw;  // Maximum retry attempts exceeded.
                }

                var delay = GetDelayWithExponentialBackoffAndJitter(attempt, initialDelayMilliseconds, maxDelayMilliseconds);

                l.A($"Transient SQL exception encountered (attempt {attempt}/{maxRetries}). Retrying after {delay} ms.");
                Thread.Sleep(delay);
            }
            catch (TimeoutException timeout)
            {
                l.Ex(timeout);

                attempt++;
                if (attempt > maxRetries)
                    throw;

                var delay = GetDelayWithExponentialBackoffAndJitter(attempt, initialDelayMilliseconds, maxDelayMilliseconds);

                l.A($"Timeout occurred (attempt {attempt}/{maxRetries}). Retrying after {delay} ms.");
                Thread.Sleep(delay);
            }
        }
    }

    /// <summary>
    /// Calculates the delay to wait using exponential backoff with jitter.
    /// The formula calculates a backoff delay that doubles with each attempt
    /// and then applies a random jitter between 0 and that delay to reduce contention.
    /// </summary>
    private static int GetDelayWithExponentialBackoffAndJitter(int attempt, int initialDelay, int maxDelay)
    {
        // Calculate exponential backoff (delay doubles with each attempt):
        var exponentialDelay = initialDelay * Math.Pow(2, attempt);
        // Ensure we don't exceed the maximum allowable delay:
        var delay = (int)Math.Min(maxDelay, exponentialDelay);
        // Apply jitter: get a random delay value between 0 and the calculated delay
        return Random.Next(0, delay);
    }

    /// <summary>
    /// Determines if a given SqlException is considered transient.
    /// Transient errors are typically temporary issues, such as resource limitations,
    /// service busy errors, or deadlocks, which might succeed when retried.
    /// </summary>
    private static bool IsTransient(SqlException ex)
        => ex.Errors.Cast<SqlError>()
            .Any(error =>

                // 4060: "Cannot open database requested by the login." 
                //       The login failed due to issues like insufficient privileges or the database being unavailable.
                error.Number == 4060

                // 10928: "Resource ID %d. The %s limit for the database is %d and has been reached." 
                //       Indicates that a resource limit (like CPU or memory) is reached.
                || error.Number == 10928

                // 10929: "Resource ID %d. The %s minimum guarantee is %d, maximum is %d and current usage is %d." 
                //       Similar to 10928, signaling a transient overload of allocated resources.
                || error.Number == 10929

                // 40197: "The service encountered an error processing your request. Please try again." 
                //       A catch-all for temporary server issues.
                || error.Number == 40197

                // 40501: "The service is currently busy. Retry the request after 10 seconds." 
                //       A clear signal that the server is under heavy load and the request should be retried later.
                || error.Number == 40501

                // 40613: "Database XXXX on server YYYY is not currently available." 
                //       Typically encountered during maintenance, failovers, or temporary service interruptions.
                || error.Number == 40613

                // 1205: "Deadlock" 
                //       Occurs when two or more transactions are deadlocked; retrying can resolve the issue.
                || error.Number == 1205
            );
}