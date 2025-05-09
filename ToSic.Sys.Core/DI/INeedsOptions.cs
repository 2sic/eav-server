namespace ToSic.Lib.DI;

public interface INeedsOptions<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Set the options for the new object
    /// </summary>
    /// <returns></returns>
    TOptions Options { get;  set; }
}