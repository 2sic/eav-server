using System;

namespace ToSic.Eav.Run.Unknown
{
    public class PlatformUnknown: PlatformInformationBase
    {
        public PlatformUnknown(WarnUseOfUnknown<PlatformUnknown> warn)  { }

        public override string Name => "Unk";

        public override Version Version => new Version(0, 0);

        public override string Identity => new Random().Next().ToString();

    }
}
