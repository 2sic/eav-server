using ToSic.Eav.Context;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Run.Unknown
{
    public sealed class ZoneCultureResolverUnknown: IZoneCultureResolver, IIsUnknown
    {
        public ZoneCultureResolverUnknown(WarnUseOfUnknown<ZoneCultureResolverUnknown> _)
        {

        }

        public string DefaultCultureCode => "en-us";
        public string CurrentCultureCode => "en-us";
    }
}
