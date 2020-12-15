using ToSic.Eav.Context;

namespace ToSic.Eav.Run.Unknown
{
    public sealed class ZoneCultureResolverUnknown: IZoneCultureResolver, IIsUnknown
    {
        public string DefaultCultureCode => "en-us";
        public string CurrentCultureCode => "en-us";
    }
}
