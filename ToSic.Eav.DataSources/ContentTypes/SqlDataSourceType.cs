using ToSic.Eav.Data.Beta_TypeSystem;

namespace ToSic.Eav.DataSources.ContentTypes
{
    [ContentTypeDefinition(CTypeName)]
    public class SqlDataSourceType: TypesBase
    {
        private const string CName = "SqlDataSource-Type";
        private const string CTypeName = "|Test.DS.SqlDataSource";

        public SqlDataSourceType() : base(CName, CTypeName)
        {
            Add(AttDef(Grp, DefInp, "ConsGrp", "Connection Information", "How to connect to the DB"));
            {
                Add(AttDef(Str, DefInp, "ConnectionStringName", "Connection Name (preferred)", "Use connection names from the web.config", "SiteSqlServer"));
                Add(AttDef(Str, DefInp, "ConnectionString", "Connection String", "Alternative: the string instead of the connection name"))
                    .StringDefault(3);
            }

            Add(AttDef(Grp, DefInp, "QryGrp", "Query", "What do you want to query, and how to treat the result"));
            { 
                Add(AttDef(Str, DefInp, "SelectCommand", "Sql Select", "Sql select command", "Select * From ..."))
                    .StringDefault(10);
                Add(AttDef(Str, DefInp, "ContentType", "Output Content Type", "Name of the content-type in result", "SqlData"));
                Add(AttDef(Str, DefInp, "TitleField", "Title Field", $"Name of the title field, if blank, defaults to {Constants.EntityFieldTitle}"));
                Add(AttDef(Str, DefInp, "IdField", "Id Field", $"Name of the ID field, if blank, defaults to {Constants.EntityFieldId}"));
            }
        }
    }
}
