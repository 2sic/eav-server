namespace ToSic.Sys.Context;

/// <summary>
/// Service to return current context information.
/// </summary>
/// <remarks>
/// Depending on the implementation, the context will contain very little, or a lot.
/// </remarks>
public interface IContextService
{
    /// <summary>
    /// The current context, containing at least the user and his permissions.
    /// Advanced implementations will return a context that has much more information, and is able to provide advanced security checks based on additional context.
    /// </summary>
    IContextOfUser Context { get; }
}
