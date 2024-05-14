using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp;

/// <summary>
/// Marks objects which can serve as a look-up source.
/// </summary>
/// <remarks>
/// Added in v17.08
/// </remarks>
[PrivateApi("Experimental")]
public interface ICanBeLookUp
{
    /// <summary>
    /// The look-up object which can be used to look-up values.
    /// </summary>
    ILookUp LookUp { get; }
}