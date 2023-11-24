using ToSic.Eav.Context;
using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Run.Unknown;

internal sealed class ZoneCultureResolverUnknown: IZoneCultureResolver, IIsUnknown
{
    public ZoneCultureResolverUnknown(WarnUseOfUnknown<ZoneCultureResolverUnknown> _)
    {

    }

    public string DefaultCultureCode => "en-us";
    public string CurrentCultureCode => "en-us";
}