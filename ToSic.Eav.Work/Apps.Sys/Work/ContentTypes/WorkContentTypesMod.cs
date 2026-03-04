using ToSic.Sys.Utils;
using static ToSic.Eav.Data.Processing.DataProcessingEvents;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkContentTypesMod(
    LazySvc<ContentTypeDataProcessorRunner> dataProcessorRunner,
    IAppReaderFactory appReaders)
    : WorkUnitBase<IAppWorkCtxWithDb>("ApS.InpGet", connect: [dataProcessorRunner, appReaders])
{
    public void Create(string nameId, string scope)
    {
        var l = Log.Fn();
        var ct = AppWorkCtx.DbStorage.ContentTypes.PrepareDbContentType(nameId, nameId, scope, false, AppWorkCtx.AppId);
        if (ct != null)
            AppWorkCtx.DbStorage.DoAndSaveWithoutChangeDetection(() => AppWorkCtx.DbStorage.SqlDb.Add(ct));
        l.Done();
    }

    public bool AddOrUpdate(string staticName, string scope, string name, int? usesConfigurationOfOtherSet,
        bool alwaysShareConfig)
    {
        var l = Log.Fn<bool>($"save {AppWorkCtx.Show()}");
        if (name.IsEmptyOrWs())
            return l.ReturnFalse("name was empty, will cancel");

        // Snapshot state before save so we can emit the most specific content-type action.
        var beforeSave = ResolveSavedContentType(staticName, scope: default, name: default);
        AppWorkCtx.DbStorage.ContentType.AddOrUpdate(staticName, scope, name, usesConfigurationOfOtherSet, alwaysShareConfig);
        // Schema changes on the type itself should immediately re-evaluate code-generation handlers.
        ProcessContentTypePostSave(staticName, scope, name, beforeSave);
        return l.ReturnTrue();
    }

    private void ProcessContentTypePostSave(string staticName, string scope, string name, IContentType? beforeSave)
    {
        // Always evaluate post-save for schema updates; handler selection happens in the runner.
        var afterSave = ResolveSavedContentType(staticName, scope, name);
        if (afterSave == null)
        {
            Log.A("Skipping content-type post-save processors because saved type could not be resolved.");
            return;
        }

        var action = DeterminePostSaveAction(beforeSave, afterSave);
        dataProcessorRunner.Value.RunFor(afterSave, action, reason: "content-type-add-or-update");
    }

    private IContentType? ResolveSavedContentType(string? staticName, string? scope, string? name)
    {
        var freshReader = appReaders.Get(AppWorkCtx.AppId);

        // StaticName is the strongest identifier and remains stable in normal rename scenarios.
        if (staticName.HasValue() && freshReader.TryGetContentType(staticName) is { } byStaticName)
            return byStaticName;

        // For pre-save snapshot calls we may only have staticName and don't want fallback matching.
        if (scope is null || name is null)
            return null;

        // Fallback by (Name + Scope) to recover edge cases where the static name changed or was not provided.
        var normalizedScope = NormalizeScope(scope);
        return freshReader.ContentTypes
            .Where(ct => ct.Name.EqualsInsensitive(name))
            .Where(ct => NormalizeScope(ct.Scope).EqualsInsensitive(normalizedScope))
            .OrderByDescending(ct => ct.Id)
            .FirstOrDefault();
    }

    private static string DeterminePostSaveAction(IContentType? beforeSave, IContentType afterSave)
    {
        if (beforeSave == null)
            return PostSaveContentTypeCreate;

        if (!beforeSave.Name.EqualsInsensitive(afterSave.Name))
            return PostSaveContentTypeRename;

        if (!NormalizeScope(beforeSave.Scope).EqualsInsensitive(NormalizeScope(afterSave.Scope)))
            return PostSaveContentTypeScopeChange;

        // Fallback for same-name/scope saves (or future AddOrUpdate expansions).
        return PostSaveContentTypeUpdate;
    }

    private static string NormalizeScope(string? scope)
        => scope?.Trim() ?? "";

    public bool CreateGhost(string sourceStaticName)
    {
        var l = Log.Fn<bool>($"create ghost a#{AppWorkCtx.Show()}, type:{sourceStaticName}");
        AppWorkCtx.DbStorage.ContentType.CreateGhost(sourceStaticName);
        return l.ReturnTrue();
    }


    public void SetTitle(int contentTypeId, int attributeId)
    {
        var l = Log.Fn($"set title type#{contentTypeId}, attrib:{attributeId}");
        AppWorkCtx.DbStorage.Attributes.SetTitleAttribute(attributeId, contentTypeId);
        l.Done();
    }

    public bool Delete(string staticName)
    {
        var l = Log.Fn<bool>($"delete a#{AppWorkCtx.Show()}, name:{staticName}");
        AppWorkCtx.DbStorage.ContentType.Delete(staticName);
        return l.ReturnTrue();
    }

}
