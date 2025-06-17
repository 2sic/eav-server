namespace ToSic.Eav.WebApi.Sys.Dto;

public class ExportPartsOverviewDto
{
    public required IEnumerable<ExportPartsContentTypesDto> ContentTypes { get; init; }
    public required IEnumerable<IdNameDto> TemplatesWithoutContentTypes { get; init; }
}

public class ExportPartsContentTypesDto: IdNameDto
{
    public required string StaticName { get; init; }
    public required IEnumerable<IdNameDto> Templates { get; init; }
    public required IEnumerable<ExportPartsEntitiesDto> Entities { get; init; }
}

public class ExportPartsEntitiesDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
}