namespace ToSic.Eav.ImportExport.Sys.XmlData;

/// <inheritdoc />
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportDataXmlPreview(AppWorkChain<ImportDataXml> contentImportXml)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtIm", connect: [contentImportXml])
{

    public ContentImportResult XmlPreview(ContentFileImportOptions args)
    {
        var l = Log.Fn<ContentImportResult>("eval content - start" + args.DebugInfo);

        var import = contentImportXml.New(MyOptions).PrepareXmlImport(args);
        if (import.ErrorLog.HasErrors)
            l.ReturnAsError(new(!import.ErrorLog.HasErrors, import.ErrorLog.Errors));

        var stats = import.Preparations;
        var result = new ContentImportResult(!import.ErrorLog.HasErrors, new ImportStatisticsDto
        {
            AmountOfEntitiesCreated = stats.EntitiesToCreate,
            AmountOfEntitiesDeleted = stats.DeleteCount,
            AmountOfEntitiesUpdated = stats.EntitiesToUpdate,
            AttributeNamesInDocument = stats.AttributeNamesInDocument,
            AttributeNamesInContentType = stats.AttributeNamesInContentType,
            AttributeNamesNotImported = stats.AttributeNamesNotImported,
            DocumentElementsCount = stats.Count,
            LanguagesInDocumentCount = stats.LanguagesInDocument.Count()
        });
        return l.Return(result);
    }

    public class ImportStatisticsDto
    {
        public required int AmountOfEntitiesCreated { get; init; }
        public required int AmountOfEntitiesDeleted { get; init; }
        public required int AmountOfEntitiesUpdated { get; init; }
        public required IEnumerable<string> AttributeNamesInDocument { get; init; }
        public required IEnumerable<string> AttributeNamesInContentType { get; init; }
        public required IEnumerable<string> AttributeNamesNotImported { get; init; }
        public required int DocumentElementsCount { get; init; }
        public required int LanguagesInDocumentCount { get; init; }
    }
}
