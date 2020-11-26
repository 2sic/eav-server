using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public interface IContextOfBlock: IRunContextCore
    {
        IPage Page { get; }

        IContainer Container { get; }

        BlockPublishingState Publishing { get; }
    }
}
