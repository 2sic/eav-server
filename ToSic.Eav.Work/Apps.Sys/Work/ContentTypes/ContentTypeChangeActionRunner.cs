using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Metadata;
using ToSic.Sys.HookUp;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Runs low-code actions for content-type schema changes.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeChangeActionRunner(
    IEnumerable<IWork<ContentTypeChange, ContentTypeChange>> actions)
    : ServiceBase("Wrk.CtAct")
{
    /// <summary>
    /// Suppression for composite operations which should only trigger once when everything is done.
    /// Same idea as DbStorage.DoButSkipAppCachePurge.
    /// </summary>
    private static readonly AsyncLocal<bool> Suppressed = new();

    /// <summary>
    /// Run something without triggering actions, so a multi-step operation can trigger once at the end.
    /// </summary>
    public void DoWithoutActions(Action action)
    {
        var before = Suppressed.Value;
        Suppressed.Value = true;
        try
        {
            action.Invoke();
        }
        finally
        {
            Suppressed.Value = before;
        }
    }

    /// <summary>
    /// Run actions for the content-types owning the fields which these metadata targets describe.
    /// Best-effort: an optional generation problem must never break the save which triggered it.
    /// </summary>
    /// <param name="prepareForRun">
    /// Called once before the first action, only if there is anything to run.
    /// Callers whose save did NOT purge the app-cache must use this to force a full reload -
    /// see the remarks, this is not optional for them.
    /// </param>
    /// <remarks>
    /// IMPORTANT: actions read field settings off the content-type, and a partial app-cache update
    /// (AppStateLoadSequence.ItemLoad) explicitly skips the content-type load - see
    /// EfcAppLoaderService.Update, "skipping content-type load". So an entity-save which only did
    /// appsCache.Update() leaves the old IContentTypeField objects in place, and generation would
    /// run against field settings that predate the save. The schema paths in WorkAttributesMod don't
    /// have this problem, because DbStorage purges the whole app on save.
    /// </remarks>
    public void RunForFieldMetadata(int appId, IEnumerable<IContentType> contentTypes, IEnumerable<ITarget> targets, Action? prepareForRun = default)
    {
        var l = Log.Fn($"app:{appId}");
        try
        {
            // Resolve first - this still needs the pre-purge state, which knows the attribute.
            // Materialize too, as actions reload the app-state we're reading here.
            var typeNames = targets
                .Where(t => t.TargetType == (int)TargetTypes.Attribute && t.KeyNumber.HasValue)
                .Select(t => contentTypes.FindAttribute(t.KeyNumber!.Value).ContentType?.NameId)
                .Where(name => name.HasValue())
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            if (typeNames.Count == 0)
            {
                l.Done("no field metadata");
                return;
            }

            prepareForRun?.Invoke();

            foreach (var typeName in typeNames)
                RunFor(appId, typeName!, ContentTypeChangeSources.ContentTypeField);

            l.Done($"types:{typeNames.Count}");
        }
        catch (Exception ex)
        {
            l.Ex("Error finding content-types for field metadata; the save itself is not affected.", ex);
            l.Done("error");
        }
    }

    public void RunFor(int appId, string contentTypeNameId, string source = ContentTypeChangeSources.ContentTypeField)
    {
        var l = Log.Fn($"app:{appId}, type:{contentTypeNameId}, source:{source}");

        if (Suppressed.Value)
        {
            l.Done("suppressed, part of a larger operation which will trigger later");
            return;
        }

        var activeActions = actions.ToList();
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
