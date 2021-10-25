using ToSic.Eav.Context;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Run.Unknown
{
    public sealed class ZoneCultureResolverUnknown: IZoneCultureResolver, IIsUnknown
    {
        public ZoneCultureResolverUnknown(WarnUseOfUnknown<ZoneCultureResolverUnknown> warn)
        {

        }

        public string DefaultCultureCode => "en-us";
        public string CurrentCultureCode => "en-us";
    }
}
