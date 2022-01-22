using System;

namespace ToSic.Eav.Run
{
    public abstract class PlatformInformationBase
    {
        public abstract string Name { get; }

        public abstract Version Version { get; }

        public abstract string Identity { get; }

    }
}
