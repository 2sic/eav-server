using ToSic.Eav.Types;
using ToSic.Eav.Types.Attributes;
using ToSic.Eav.Types.Builder;

namespace ToSic.Eav.DataSources.ContentTypes
{
    [ContentTypeDefinition(StaticTypeName)]
    public class ConfigSqlDataSource: TypesBase
    {
        internal const string StaticTypeName = "|Config ToSic.Eav.DataSources.SqlDataSource";

        public ConfigSqlDataSource() : base(StaticTypeName, StaticTypeName, Constants.ScopeSystem, StaticTypeName.ToLowerInvariant().TrimStart('|'))
        {
            Add(AttDef(Str, DefInp, "Title", defaultValue: "Sql Query")
                .MakeTitle());
            Add(AttDef(Grp, DefInp, "ConnectionGroup"));
            {
                Add(AttDef(Str, DefInp, "ConnectionStringName", defaultValue: "SiteSqlServer"));
                Add(AttDef(Str, DefInp, "ConnectionString"))
                    .StringDefault(3);
            }

            Add(AttDef(Grp, DefInp, "QueryGroup", "Query"));
            { 
                Add(AttDef(Str, DefInp, "SelectCommand", defaultValue: "Select * From ..."))
                    .StringDefault(10);
                Add(AttDef(Str, DefInp, "ContentType", defaultValue: "SqlData"));
                Add(AttDef(Str, DefInp, "TitleField"));
                Add(AttDef(Str, DefInp, "IdField"));
            }

            this.ContentTypeMetadata("SQL DataSource");
        }
    }
}
