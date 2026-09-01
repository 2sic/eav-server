namespace ToSic.Eav.Data.Raw;

/// <summary>
/// WIP v22 2dm
/// </summary>
[WorkInProgressApi("v22")]
public interface IRawEntityConvertible: IRawData
{
    /// <summary>
    /// Get a raw-entity converter for this object.
    /// </summary>
    /// <remarks>
    /// Must be a method, to better shield it from serialization issues.
    /// If it was a property, it would be serialized and then deserialized, which could cause problems.
    ///
    /// We also recommend any implementations being explicit, so it doesn't show up in the API or intellisense.
    /// </remarks>
    /// <returns>An instance of the converter or if it should use the default, returns null.</returns>
    IRawEntityConverter GetConverter();
}