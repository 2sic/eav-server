using ToSic.Eav.Data.Processing;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Runs low-code actions for content-type schema changes.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeChangeActionRunner(
    IEnumerable<ILowCodeAction<ContentTypeChange, ContentTypeChange>> actions)
    : ServiceBase("Wrk.CtAct")
{
    public void RunFor(int appId, int contentTypeId, string source = ContentTypeChangeSources.ContentTypeField)
    {
        var activeActions = actions.ToList();
        var l = Log.Fn($"app:{appId}, typeId:{contentTypeId}, source:{source}, actions:{activeActions.Count}");

        if (activeActions.Count == 0)
        {
            l.Done("no actions");
            return;
        }

        var actionContext = new LowCodeActionContext();
        var result = ActionData.Create(new ContentTypeChange(
            AppId: appId,
            ContentTypeId: contentTypeId,
            Source: source));

        foreach (var action in activeActions)
        {
            try
            {
                var errorCountBefore = result.Exceptions.Count;
                result = action
                    .Run(actionContext, result)
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
