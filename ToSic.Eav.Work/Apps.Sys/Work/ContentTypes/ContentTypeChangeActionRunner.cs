using ToSic.Eav.Data.Processing;
using ToSic.Sys.HookUp;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Runs low-code actions for content-type schema changes.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeChangeActionRunner(
    IEnumerable<IWork<ContentTypeChange, ContentTypeChange>> actions)
    : ServiceBase("Wrk.CtAct")
{
    public void RunFor(int appId, string contentTypeNameId, string source = ContentTypeChangeSources.ContentTypeField)
    {
        var activeActions = actions.ToList();
        var l = Log.Fn($"app:{appId}, type:{contentTypeNameId}, source:{source}, actions:{activeActions.Count}");

        if (activeActions.Count == 0)
        {
            l.Done("no actions");
            return;
        }

        var actionContext = new WorkContext();
        var result = new ContentTypeChange(AppId: appId, ContentTypeNameId: contentTypeNameId, Source: source)
            .ToPackage();

        foreach (var action in activeActions)
        {
            try
            {
                var errorCountBefore = result.Exceptions.Count;
                result = action
                    .Handle(actionContext, result)
                    .GetAwaiter()
                    .GetResult();

                var addedErrors = result.Exceptions.Count - errorCountBefore;
                if (addedErrors > 0)
                    l.A($"Action '{action.GetType().Name}' reported {addedErrors} exception(s).");
            }
            catch (Exception ex)
            {
                // Schema saves must not fail because optional low-code actions failed.
                l.Ex(ex, $"Error in content-type change action '{action.GetType().FullName}'.");
            }
        }

        l.Done();
    }
}
