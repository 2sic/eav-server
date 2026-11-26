namespace ToSic.Eav.Context.Sys.ZoneCulture;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class ZoneCultureResolverUnknown(WarnUseOfUnknown<ZoneCultureResolverUnknown> _) : IZoneCultureResolver, IIsUnknown
{
    public string DefaultCultureCode => "en-us";
    public string CurrentCultureCode => "en-us";
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public record ZoneCultureInfo(string CurrentCultureCode, string DefaultCultureCode) : IZoneCultureResolver
{

}