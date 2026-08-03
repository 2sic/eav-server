namespace ToSic.Eav.Models.Sys;

internal class ModelContentTypeNameCacheKeys
{
    internal const string CachePrefixType = "type";
    internal const string CachePrefixDerived = "derived";
    
    internal static string CacheKeyOptions(string optionsTypeName, string nameId = "") => $"option:{optionsTypeName}|{nameId}";

    // We only need the initial type, because even if it's an interface, it will always result in the same concrete type
    internal static string CacheKeyForType(string prefix, Type entryType, string nameId = "") => $"{prefix}:{entryType.FullName}|{nameId}";
}