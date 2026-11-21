using ToSic.Eav.Ui.Edit.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.History.Sys;

public interface IHistoryController: IHasLog
{
    /// <summary>
    /// Restore an item to a previous version in the history
    /// </summary>
    // UI test: Open an item's History dialog and restore a previous version.
    bool Restore(int appId, int transactionId, ItemIdentifier item);
}
