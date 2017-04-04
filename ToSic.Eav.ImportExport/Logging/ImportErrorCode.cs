using ToSic.Eav.ImportExport.Refactoring;

namespace ToSic.Eav.ImportExport.Logging
{
    public enum ImportErrorCode
    {
        [LocalizedDescription("Unknown", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        Unknown,

        [LocalizedDescription("InvalidContentType", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        InvalidContentType,

        [LocalizedDescription("InvalidDocument", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        InvalidDocument,

        [LocalizedDescription("InvalidRoot", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        InvalidRoot,

        [LocalizedDescription("InvalidLanguage", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        InvalidLanguage,

        [LocalizedDescription("MissingElementLanguage", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        MissingElementLanguage,

        [LocalizedDescription("InvalidValueReference", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        InvalidValueReference,

        [LocalizedDescription("InvalidValueReferenceProtection", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        InvalidValueReferenceProtection,

        [LocalizedDescription("InvalidValueFormat", typeof(ImportErrorCode), "ToSic.Eav.ImportExport.Refactoring")]
        InvalidValueFormat,
    }
}