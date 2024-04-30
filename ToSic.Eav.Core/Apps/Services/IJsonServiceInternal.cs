using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Services;

/// <summary>
/// TODO: @STV COMMENT WHY THIS EXISTS
/// </summary>
[PrivateApi]
public interface IJsonServiceInternal
{
    string ToJson(object item);

    string ToJson(object item, int indentation);

    T To<T>(string json);

    object ToObject(string json);
}