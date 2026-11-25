using System.Text;

namespace ToSic.Sys.Utils;

/// <summary>
/// https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class Base64
{
    public static string? Utf8ToBase64(this string? plainText)
    {
        if (plainText == null)
            return null;
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    [return: NotNullIfNotNull(nameof(base64EncodedData))]
    public static string? Base64ToUtf8(this string? base64EncodedData)
    {
        if (base64EncodedData == null)
            return null;
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}