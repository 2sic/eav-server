using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys;

/// <summary>
/// Marks helper services which either correct, add or remove identifiers to load for the Edit UI.
/// </summary>
public interface IEditLoadIdentifierProvider:
    IServiceWithSetup<IAppWorkContext>,
    IWork<List<ItemIdentifier>>,
    IWorkSequenceOrder;