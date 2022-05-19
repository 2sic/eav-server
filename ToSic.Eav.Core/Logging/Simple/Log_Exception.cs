using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        private const int MaxExceptionRecursion = 100;

        public void Exception(Exception ex)
        {
            var wrapLog = Call(message: "Will log Exception Details next");
            var recursion = 1;
            while (true)
            {
                // avoid infinite loops
                if (recursion >= MaxExceptionRecursion)
                {
                    wrapLog("max-depth reached");
                    return;
                }

                if (ex == null)
                {
                    wrapLog("Exception is null");
                    return;
                }

                this.A($"Depth {recursion} in {ex.Source}: {ex}"); // use the default ToString of an exception

                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    recursion++;
                    continue;
                }

                break;
            }
            wrapLog(null);
        }
    }
}
