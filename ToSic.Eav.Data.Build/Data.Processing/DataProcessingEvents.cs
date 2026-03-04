namespace ToSic.Eav.Data.Processing;

public class DataProcessingEvents
{
    public const string PreSave = "pre-save";
    public const string PreEdit = "pre-edit";
    public const string PostSave = "post-save";

    // Content-type specific post-save actions.
    // These are intentionally separate from PostSave so schema processors
    // don't run on normal entity data save operations.
    public const string PostSaveContentTypeCreate = "post-save-content-type-create";
    public const string PostSaveContentTypeRename = "post-save-content-type-rename";
    public const string PostSaveContentTypeScopeChange = "post-save-content-type-scope-change";
    public const string PostSaveContentTypeFieldChange = "post-save-content-type-field-change";
    public const string PostSaveContentTypeUpdate = "post-save-content-type-update";
}
