namespace ToSic.Eav.ImportExport.Sys;

public record AppExportSpecs(
    int ZoneId,
    int AppId,
    bool IncludeContentGroups = false,
    bool ResetAppGuid = false,
    bool AssetsAdam = true,
    bool AssetsSite = true,
    bool AssetAdamDeleted = true,
    bool WithSiteFiles = false
)
{
    public string Dump() => $"ZoneId: {ZoneId}, AppId: {AppId}, IncludeContentGroups: {IncludeContentGroups}, ResetAppGuid: {ResetAppGuid}, AssetsAdam: {AssetsAdam}, AssetsSite: {AssetsSite}, AssetAdamDeleted: {AssetAdamDeleted}, WithSiteFiles: {WithSiteFiles}";
}