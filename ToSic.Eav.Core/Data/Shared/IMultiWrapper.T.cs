using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Marks items which can be rewrapped multiple times.
/// Important to keep track of the originally wrapped item to ensure equality checks
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IMultiWrapper<out T>
{
    /// <summary>
    /// The underlying wrapped object which is used for equality check.
    /// It's important, because the Entity can sometimes already be wrapped
    /// in which case the various wrappers would think they point
    /// to something different
    /// </summary>
    T RootContentsForEqualityCheck { get; }

}