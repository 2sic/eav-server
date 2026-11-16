namespace ToSic.Eav.WebApi.Admin.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppCacheFlushSpecs
{
    // Null or empty means app-wide LightSpeed invalidation; otherwise only the named dependencies are touched.
    public string[]? Dependencies { get; set; }
}
