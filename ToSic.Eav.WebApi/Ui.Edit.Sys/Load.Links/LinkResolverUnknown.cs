using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Links;

internal class LinkResolverUnknown(WarnUseOfUnknown<LinkResolverUnknown> _) : ILinkResolver
{
    public async Task<Package<LinkInfoDto>> Handle(WorkContext mainCtx, Package<ILinkResolver.Parameters> package)
    {
        return new LinkInfoDto { Value = package.Data.Hyperlink }.ToPackage();
    }
}
