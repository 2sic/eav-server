using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class ContextOfBlock: ContextOfSite, IContextOfBlock
    {
        public ContextOfBlock(ISite site, IPage page, IContainer container, IUser user, IServiceProvider serviceProvider,
            BlockPublishingState publishing): base(serviceProvider, site, user)
        {
            Page = page;
            Container = container;
            Publishing = publishing;
        }


        public IPage Page { get; protected set; }
        public IContainer Container { get; }
        public BlockPublishingState Publishing { get; }

        public new IContextOfSite Clone() => new ContextOfBlock(Site, Page, Container, User, ServiceProvider, Publishing);
    }
}
