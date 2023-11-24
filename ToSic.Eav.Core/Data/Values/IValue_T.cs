using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a Value with a specific type (string, decimal, etc.).
/// </summary>
/// <typeparam name="T">Type of the actual Value</typeparam>
[PublicApi_Stable_ForUseInYourCode]
public interface IValue<out T> : IValue
{
    /// <summary>
    /// Typed contents of the value
    /// </summary>
    T TypedContents { get; }
}