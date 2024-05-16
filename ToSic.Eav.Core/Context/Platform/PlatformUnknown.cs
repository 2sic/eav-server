using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Context;

internal class PlatformUnknown: IPlatformInfo
{
    public PlatformUnknown(WarnUseOfUnknown<PlatformUnknown> _)  { }

    public string Name => "Unk";

    public Version Version => new(0, 0);

    public string Identity => new Random().Next().ToString();

}