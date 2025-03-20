namespace ToSic.Eav.LookUp;

public static class LookUpTestAccessors
{
    public static string GetTac(this ILookUp lookUp, string key) => lookUp.Get(key);

    public static string GetTac(this ILookUp lookUp, string key, string format) => lookUp.Get(key, format);
}