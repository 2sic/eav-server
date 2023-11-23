using System;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Context
{
    public class PlatformUnknown: IPlatformInfo
    {
        public PlatformUnknown(WarnUseOfUnknown<PlatformUnknown> _)  { }

        public string Name => "Unk";

        public Version Version => new(0, 0);

        public string Identity => new Random().Next().ToString();

    }
}
