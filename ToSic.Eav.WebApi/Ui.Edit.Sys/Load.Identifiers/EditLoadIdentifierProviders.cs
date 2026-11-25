using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys;

/// <summary>
/// Process all identifier providers.
/// </summary>
/// <param name="workSequence"></param>
public class EditLoadIdentifierProviders(IWorkSequence<IEditLoadIdentifierProvider, List<ItemIdentifier>> workSequence)
    : ServiceBase("UoW.AddCtx", connect: [workSequence]),
        IWork<List<ItemIdentifier>>
{
    public async Task<Package<List<ItemIdentifier>>> Handle(WorkContext actionCtx, Package<List<ItemIdentifier>> package)
    {
        var l = Log.Fn<Package<List<ItemIdentifier>>>();

        var result = await workSequence.Handle(actionCtx, package);

        return l.Return(result);
    }
}
