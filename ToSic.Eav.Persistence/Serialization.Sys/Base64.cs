using System.Text;

namespace ToSic.Eav.Serialization;

/// <summary>
/// https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class Base64
{
    public static string Encode(string plainText)
    {
        if(plainText == null) return null;
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string Decode(string base64EncodedData)
    {
        if (base64EncodedData == null) return null;
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}