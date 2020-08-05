using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Dto
{
    public class ExportPartsOverviewDto
    {
        public IEnumerable<ExportPartsContentTypesDto> ContentTypes;
        public IEnumerable<ExportPartsIdNameDto> TemplatesWithoutContentTypes;
    }

    public class ExportPartsContentTypesDto: ExportPartsIdNameDto
    {
        public string StaticName;
        public IEnumerable<ExportPartsIdNameDto> Templates;
        public IEnumerable<ExportPartsEntitiesDto> Entities;
    }

    public class ExportPartsIdNameDto
    {
        public int Id;
        public string Name;
    }

    public class ExportPartsEntitiesDto
    {
        public int Id;
        public string Title;
    }
}
