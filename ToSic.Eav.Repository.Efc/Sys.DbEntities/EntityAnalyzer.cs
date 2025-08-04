using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;
internal static class EntityAnalyzer
{
    internal static bool UseJson(this IEntity newEnt) => newEnt.Type.RepositoryType != RepositoryTypes.Sql;

}
