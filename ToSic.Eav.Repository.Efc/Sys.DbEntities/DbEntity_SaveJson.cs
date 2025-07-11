using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{

    internal string GenerateJsonOrReportWhyNot(IEntity newEnt, bool logDetails)
    {
        string jsonExport;
        if (logDetails)
            LogDetails.A($"will serialize-json id:{newEnt.EntityId}");
        try
        {
            jsonExport = Serializer.Serialize(newEnt);
        }
        catch
        {
            if (logDetails)
                LogDetails.A("Error serializing - will repeat with detailed with logging");
            Serializer.LinkLog(Log);
            jsonExport = Serializer.Serialize(newEnt);
        }

        return jsonExport;
    }

    private void DropEavAttributesForJsonItem(IEntity newEnt)
    {
        // in update scenarios, the old data could have been a db-model, so clear that
        ClearAttributesInDbModel(newEnt.EntityId);
        DbContext.Relationships.FlushChildrenRelationships([newEnt.EntityId]);
    }
}