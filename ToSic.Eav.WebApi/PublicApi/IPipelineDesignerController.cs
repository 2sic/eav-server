using System.Collections.Generic;
using System.Web.Http;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IPipelineDesignerController
    {
        void ClonePipeline(int appId, int id);
        object DeletePipeline(int appId, int id);
        IEnumerable<QueryRuntime.DataSourceInfo> GetInstalledDataSources();
        QueryDefinitionInfo GetPipeline(int appId, int? id = null);
        bool ImportQuery(EntityImport args);
        dynamic QueryPipeline(int appId, int id);
        QueryDefinitionInfo SavePipeline([FromBody] QueryDefinitionInfo data, int appId, int id);
    }
}