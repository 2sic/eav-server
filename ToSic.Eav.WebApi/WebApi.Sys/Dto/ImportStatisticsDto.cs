namespace ToSic.Eav.WebApi.Sys.Dto;

public class ImportStatisticsDto
{
    public required int AmountOfEntitiesCreated{ get; init; }
    public required int AmountOfEntitiesDeleted{ get; init; }
    public required int AmountOfEntitiesUpdated{ get; init; }
    public required IEnumerable<string> AttributeNamesInDocument{ get; init; }
    public required IEnumerable<string> AttributeNamesInContentType{ get; init; }
    public required IEnumerable<string> AttributeNamesNotImported{ get; init; }
    public required int DocumentElementsCount{ get; init; }
    public required int LanguagesInDocumentCount{ get; init; }
}