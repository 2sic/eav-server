#if NETFRAMEWORK
namespace ToSic.Eav.Data;

partial record EntityLight
{
    /// <inheritdoc />
    [PrivateApi]
    [Obsolete("Deprecated. Do not use any more. Hyperlinks won't be resolved")]
    public object GetBestValue(string attributeName, bool resolveHyperlinks) => GetBestValue(attributeName);
}

#endif