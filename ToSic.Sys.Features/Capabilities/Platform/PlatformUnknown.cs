using ToSic.Eav.Internal.Unknown;

namespace ToSic.Sys.Capabilities.Platform;

internal class PlatformUnknown(WarnUseOfUnknown<PlatformUnknown> _): IPlatformInfo
{
    public string Name => "Unk";

    public Version Version => new(0, 0);

    public string Identity => new Random().Next().ToString();

}