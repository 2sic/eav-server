using ToSic.Eav.Data.Processing;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Runs low-code actions for content-type schema changes.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeChangeActionRunner(
    IAppReaderFactory appReaders,
    IEnumerable<ILowCodeAction<ContentTypeChange, ContentTypeChange>> actions)
    : ServiceBase("Wrk.CtAct", connect: [appReaders])
{
    public void RunFor(IContentType contentType, string source = ContentTypeChangeSources.ContentType)
    {
        var l = Log.Fn($"type:{contentType.NameId}, app:{contentType.AppId}, source:{source}");

        // Always re-resolve from a fresh app reader so actions see committed DB state.
        var freshReader = appReaders.Get(contentType.AppId);
        var freshType = freshReader.TryGetContentType(contentType.NameId)
                        ?? freshReader.GetContentTypeOptional(contentType.Id);
        if (freshType == null)
        {
            l.A("Skip - content-type could not be resolved from app state.");
            l.Done();
            return;
        }

        RunForResolvedContentType(freshType, source);
        l.Done();
    }

    public void RunFor(int appId, int contentTypeId, string source = ContentTypeChangeSources.ContentTypeField)
    {
        var l = Log.Fn($"app:{appId}, typeId:{contentTypeId}, source:{source}");
        var contentType = appReaders.Get(appId).GetContentTypeOptional(contentTypeId);
        if (contentType == null)
        {
            l.A("Skip - content-type id could not be resolved.");
            l.Done();
            return;
        }

        RunForResolvedContentType(contentType, source);
        l.Done();
    }

    private void RunForResolvedContentType(IContentType contentType, string source)
    {
        var activeActions = actions.ToList();
        var l = Log.Fn($"type:{contentType.NameId}, app:{contentType.AppId}, source:{source}, actions:{activeActions.Count}");

        if (!activeActions.Any())
        {
            l.Done("no actions");
            return;
        }

        var actionContext = new LowCodeActionContext();
        var result = ActionData.Create(new ContentTypeChange(
            AppId: contentType.AppId,
            ContentTypeId: contentType.Id,
            ContentTypeNameId: contentType.NameId,
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
