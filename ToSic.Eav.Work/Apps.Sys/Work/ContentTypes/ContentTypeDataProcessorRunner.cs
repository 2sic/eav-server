using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Models;
using ToSic.Sys.Utils;
using ToSic.Sys.Utils.Assemblies;
using static ToSic.Eav.Data.Processing.DataProcessingEvents;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Runs post-save data processors for content-type schema changes.
/// This is best-effort by design and must never block save operations.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeDataProcessorRunner(
    IServiceProvider serviceProvider,
    IAppReaderFactory appReaders)
    : ServiceBase("Wrk.CtProc", connect: [appReaders])
{
    /// <summary>
    /// Execute a data processor action for a known content-type using the standard
    /// action verbs and optional schema context.
    /// </summary>
    public void RunFor(IContentType contentType, string action = PostSave, DataProcessingContext? context = default)
    {
        var l = Log.Fn($"type:{contentType.NameId}, app:{contentType.AppId}, action:{action}, source:{context?.Source}");

        // Always re-resolve from a fresh app reader so metadata/decorators reflect committed DB state.
        var freshReader = appReaders.Get(contentType.AppId);
        var freshType = freshReader.TryGetContentType(contentType.NameId)
                        ?? freshReader.GetContentTypeOptional(contentType.Id);
        if (freshType == null)
        {
            l.A("Skip - content-type could not be resolved from app state.");
            l.Done();
            return;
        }

        RunForResolvedContentType(freshType, action, context);
        l.Done();
    }

    /// <summary>
    /// Execute a data processor action for a content-type identified by app and numeric id.
    /// </summary>
    public void RunFor(int appId, int contentTypeId, string action = PostSave, DataProcessingContext? context = default)
    {
        var l = Log.Fn($"app:{appId}, typeId:{contentTypeId}, action:{action}, source:{context?.Source}");
        // Some callers only know numeric ids (field mutations), so this overload is the central bridge.
        var contentType = appReaders.Get(appId).GetContentTypeOptional(contentTypeId);
        if (contentType == null)
        {
            l.A("Skip - content-type id could not be resolved.");
            l.Done();
            return;
        }

        RunForResolvedContentType(contentType, action, context);
        l.Done();
    }

    private void RunForResolvedContentType(IContentType contentType, string action, DataProcessingContext? context)
    {
        var l = Log.Fn($"type:{contentType.NameId}, app:{contentType.AppId}, action:{action}, source:{context?.Source}");

        if (action.IsEmptyOrWs())
        {
            l.A("Skip - content-type processor action was empty.");
            l.Done();
            return;
        }

        // Decorator is the opt-in marker for content-type level post-save processing.
        var decorator = contentType.GetMetadataModel<DataStorageDecorator>();
        if (decorator == null || decorator.DataProcessingHandler.IsEmptyOrWs())
        {
            l.A("Skip - no DataProcessingHandler decorator configured.");
            l.Done();
            return;
        }

        var handlerType = AssemblyHandling.GetTypeOrNull(decorator.DataProcessingHandler);
        if (handlerType == null)
        {
            l.A($"Skip - handler type '{decorator.DataProcessingHandler}' not found.");
            l.Done();
            return;
        }

        if (!typeof(IDataProcessor).IsAssignableFrom(handlerType))
        {
            // Safety guard: metadata points to an arbitrary type name, but only IDataProcessor is allowed.
            l.A($"Skip - handler '{handlerType.FullName}' is not an IDataProcessor.");
            l.Done();
            return;
        }

        if (serviceProvider.GetService(handlerType) is not IDataProcessor processor)
        {
            l.A($"Skip - handler '{handlerType.FullName}' could not be resolved from DI.");
            l.Done();
            return;
        }

        try
        {
            // The resolved content-type is the source of truth for schema processors.
            var enrichedContext = (context ?? new DataProcessingContext()) with
            {
                AppId = contentType.AppId,
                ContentTypeId = contentType.Id,
                ContentTypeNameId = contentType.NameId
            };

            // Use the standard IDataProcessor contract with the shared action verb plus
            // explicit context so processors can distinguish schema triggers from entity saves.
            var result = processor
                .Process(action, new DataProcessorResult<IEntity?> { Data = null, Context = enrichedContext })
                .GetAwaiter()
                .GetResult();

            if (result.Exceptions.Any())
                l.A($"Handler '{handlerType.Name}' reported {result.Exceptions.Count} exception(s).");
        }
        catch (Exception ex)
        {
            // Intentionally non-blocking: schema save must not fail because generation failed.
            l.Ex(ex, $"Error in post-save handler '{handlerType.FullName}'.");
        }

        l.Done();
    }
}
