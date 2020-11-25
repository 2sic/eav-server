using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public interface IInstanceContext
    {
        ISite Site { get; }

        IPage Page { get; }

        IContainer Container { get; }

        IUser User { get; }

        IInstanceContext Clone(ISite site = null, IPage page = null, IContainer container = null, IUser user = null);

        IServiceProvider ServiceProvider { get; }

        InstancePublishingState Publishing { get; }

        //List<KeyValuePair<string, string>> PageParameters { get; }
    }
}
