using System.Collections.Generic;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IQueryController
    {
        void Clone(int appId, int id);
        bool Delete(int appId, int id);
        IEnumerable<DataSourceDto> DataSources();
        QueryDefinitionDto Get(int appId, int? id = null);
        bool Import(EntityImportDto args);
        QueryRunDto Run(int appId, int id, int top = 0);
        QueryDefinitionDto Save(QueryDefinitionDto data, int appId, int id);

        QueryRunDto DebugStream(int appId, int id, string from, string @out, int top = 25);
    }
}