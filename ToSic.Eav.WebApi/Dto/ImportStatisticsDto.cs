namespace ToSic.Eav.WebApi.Dto;

public class ImportStatisticsDto
{
    public int AmountOfEntitiesCreated;
    public int AmountOfEntitiesDeleted;
    public int AmountOfEntitiesUpdated;
    public IEnumerable<string> AttributeNamesInDocument;
    public IEnumerable<string> AttributeNamesInContentType;
    public IEnumerable<string> AttributeNamesNotImported;
    public int DocumentElementsCount;
    public int LanguagesInDocumentCount;
}