namespace ToSic.Sys.Utils;

public static class Retry
{
    public static bool RetryOnException(Action action, ILogCall logCall, int repeat = 10, int delay = 100,
        bool silent = false)
    {
        var l = logCall.Fn<bool>($"{nameof(repeat)}: {repeat}; {nameof(delay)}: {delay}; {nameof(silent)}: {silent}");
        for (var i = 0; i < repeat; i++)
        {
            try
            {
                action();
                return l.ReturnTrue();
            }
            catch (Exception ex)
            {
                // give up after 5 tries
                if (i >= repeat - 1)
                {
                    var msg = $"Failed after {repeat} tries, giving up.";
                    l.Ex(ex);
                    if (silent)
                        return l.ReturnFalse(msg);

                    l.Done("Failed after 10 tries, giving up.");
                    throw;
                }

                // otherwise do a short delay to try again
                l.A($"Attempt {i} failed, delay and try again.");
                Thread.Sleep(i * delay);
            }
        }

        return l.ReturnFalse("unclear what happened, this should not occur");
    }
}
