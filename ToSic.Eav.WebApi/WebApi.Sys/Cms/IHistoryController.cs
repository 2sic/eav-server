using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Cms;

public interface IHistoryController: IHasLog
{
    /// <summary>
    /// Restore an item to a previous version in the history
    /// </summary>
    bool Restore(int appId, int transactionId, ItemIdentifier item);
}