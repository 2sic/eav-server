using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        private static bool UseJson(IEntity newEnt) => newEnt.Type.RepositoryType != RepositoryTypes.Sql;

        private string GenerateJsonOrReportWhyNot(IEntity newEnt, bool logDetails)
        {
            string jsonExport;
            if (logDetails) Log.Add($"will serialize-json id:{newEnt.EntityId}");
            try
            {
                jsonExport = _serializer.Serialize(newEnt);
            }
            catch
            {
                if (logDetails) Log.Add("Error serializing - will repeat with detailed with logging");
                _serializer.LinkLog(Log);
                jsonExport = _serializer.Serialize(newEnt);
            }

            return jsonExport;
        }

        private void DropEavAttributesForJsonItem(IEntity newEnt)
        {
            // in update scenarios, the old data could have been a db-model, so clear that
            ClearAttributesInDbModel(newEnt.EntityId);
            DbContext.Relationships.FlushChildrenRelationships(new List<int> { newEnt.EntityId });
        }
    }
}
