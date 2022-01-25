using System;

namespace ToSic.Eav.Run.Unknown
{
    public class PlatformUnknown: IPlatformInfo
    {
        public PlatformUnknown(WarnUseOfUnknown<PlatformUnknown> warn)  { }

        public string Name => "Unk";

        public Version Version => new Version(0, 0);

        public string Identity => new Random().Next().ToString();

    }
}
