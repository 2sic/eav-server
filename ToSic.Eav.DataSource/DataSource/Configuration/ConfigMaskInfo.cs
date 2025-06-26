namespace ToSic.Eav.DataSource;

[PrivateApi]
internal record ConfigMaskInfo
{
    public required string Key;
    public required string Token;
    public required bool CacheRelevant;
}