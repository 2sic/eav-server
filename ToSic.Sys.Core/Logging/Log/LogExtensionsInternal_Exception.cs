namespace ToSic.Lib.Logging;

partial class LogExtensionsInternal
{
    private const int MaxExceptionRecursion = 100;

    internal static TException ExceptionInternal<TException>(this ILog log, TException ex, CodeRef codeRef) where TException : Exception
    {
        // Null-check
        if (log.GetRealLog() is not Log realLog) return ex;

        var l = realLog.FnCode(message: $"{LogConstants.ErrorPrefix}Will log Exception Details next", code: codeRef);
        var recursion = 1;
        Exception loopEx = ex;
        while (true)
        {
            // avoid infinite loops
            if (recursion >= MaxExceptionRecursion)
            {
                l.Done("max-depth reached");
                return ex;
            }
                
            if (ex == null)
            {
                l.Done("Exception is null");
                return ex;
            }

            realLog.A($"Depth {recursion} in {loopEx.Source}: {loopEx}"); // use the default ToString of an exception

            if (loopEx.InnerException != null)
            {
                loopEx = loopEx.InnerException;
                recursion++;
                continue;
            }

            break;
        }
        l.Done();
        return ex;
    }
}