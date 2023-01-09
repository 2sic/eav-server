using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// A mini logger for a function call, which should be closed using a form of `Return(...)` when the function completes.
    /// </summary>
    /// <remarks>
    /// 1. It's important to note that all Return commands are extension methods.
    /// 1. Certain types such as `bool` have their own custom `Return...` commands, such as `ReturnFalse()`
    /// </remarks>
    /// <typeparam name="T">
    /// Type of data to return at the end of the call.
    /// Note that you cannot use `dynamic` for T, so if your result is dynamic, use `object`
    /// </typeparam>
    [PublicApi]
    public interface ILogCall<T>: ILogCall
    {
    }
}
