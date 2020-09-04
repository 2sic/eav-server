using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Dto
{
    public class ExportPartsOverviewDto
    {
        public IEnumerable<ExportPartsContentTypesDto> ContentTypes;
        public IEnumerable<IdNameDto> TemplatesWithoutContentTypes;
    }

    public class ExportPartsContentTypesDto: IdNameDto
    {
        public string StaticName;
        public IEnumerable<IdNameDto> Templates;
        public IEnumerable<ExportPartsEntitiesDto> Entities;
    }

    public class ExportPartsEntitiesDto
    {
        public int Id;
        public string Title;
    }
}
