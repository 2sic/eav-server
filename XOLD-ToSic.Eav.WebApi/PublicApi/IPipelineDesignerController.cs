using System.Collections.Generic;
using System.Web.Http;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IPipelineDesignerController
    {
        void ClonePipeline(int appId, int id);
        object DeletePipeline(int appId, int id);
        IEnumerable<QueryRuntime.DataSourceInfo> GetInstalledDataSources();
        QueryDefinitionDto GetPipeline(int appId, int? id = null);
        bool ImportQuery(EntityImportDto args);
        QueryRunDto QueryPipeline(int appId, int id);
        QueryDefinitionDto SavePipeline([FromBody] QueryDefinitionDto data, int appId, int id);
    }
}