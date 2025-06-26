using ToSic.Lib.Services;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Sys.Capabilities.Platform;

internal class PlatformUnknown(WarnUseOfUnknown<PlatformUnknown> _): IPlatformInfo
{
    public string Name => "Unk";

    public Version Version => new(0, 0);

    public string Identity => new Random().Next().ToString();

}