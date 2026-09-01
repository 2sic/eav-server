namespace ToSic.Sys.HookUp;

/// <summary>
/// WIP
/// </summary>
public interface IWorkSequenceOrder
{
    /// <summary>
    /// Order to execute the work in, ascending (smaller is better).
    /// Implementations will be cycled through according to this order.
    /// </summary>
    int WorkSequenceOrder { get; }
}