using ToSic.Eav.Data.Versions.Sys;
using ToSic.Eav.Ui.Edit.Sys;

namespace ToSic.Eav.Ui.History.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class HistoryControllerReal(AppWorkQuick<WorkEntityVersionRestore> versioning)
    : ServiceBase("Api.CmsHistoryRl", connect: [versioning])
{
    public const string LogSuffix = "Hist";


    public bool Restore(int appId, int transactionId, ItemIdentifier item)
    {
        versioning.New(appId).VersionRestore(item.EntityId, transactionId);
        return true;
    }
}