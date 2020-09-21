using System.Collections.Generic;
#if NET451
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IPipelineDesignerController
    {
        void Clone(int appId, int id);
        bool Delete(int appId, int id);
        IEnumerable<QueryRuntime.DataSourceInfo> DataSources();
        QueryDefinitionDto Get(int appId, int? id = null);
        bool Import(EntityImportDto args);
        QueryRunDto Run(int appId, int id);
        QueryDefinitionDto Save([FromBody] QueryDefinitionDto data, int appId, int id);
    }
}