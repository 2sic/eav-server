namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static partial class ILogExtensions
    {




        ///// <summary>
        ///// Short wrapper for Get-property calls which return the value, and log the result.
        ///// </summary>
        ///// <typeparam name="TResult">Type of return value</typeparam>
        ///// <returns></returns>
        //[PrivateApi]
        //public static TResult GetAndLog<TResult>(this ILog log,
        //    Func<ILogCall<TResult>, TResult> action,
        //    bool timer = default,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.WrapValueOnly(action, true, true, null, null, timer, Create(cPath, cName, cLine));

        ///// <summary>
        ///// Short wrapper for Get-property calls which return the value, and log the result and a message
        ///// </summary>
        ///// <typeparam name="TResult">Type of return value</typeparam>
        ///// <returns></returns>
        //[PrivateApi]
        //public static TResult GetAndLog<TResult>(this ILog log,
        //    Func<ILogCall<TResult>, (TResult Result, string Message)> action,
        //    bool timer = default,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.WrapValueAndMessage(action, true, true, null, null, timer, Create(cPath, cName, cLine));






    }
}
