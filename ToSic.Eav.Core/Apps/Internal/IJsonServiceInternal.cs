namespace ToSic.Eav.Apps.Internal;

/// <summary>
/// Dependency of ToSic.Eav.Apps.Internal.AppJsonService to convert JSON to typed object.
/// </summary>
/// <remarks>
/// Full IJsonService is not in EAV
/// </remarks>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IJsonServiceInternal
{
    /// <summary>
    /// Convert a JSON to a typed object. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    T To<T>(string json);
}