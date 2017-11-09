//using ToSic.Eav.Types;
//using ToSic.Eav.Types.Attributes;
//using ToSic.Eav.Types.Builder;

//namespace ToSic.Eav.DataSources.ContentTypes
//{
//    [ContentTypeDefinition(StaticTypeName)]
//    public class ConfigSqlDataSource: TypesBase
//    {
//        internal const string StaticTypeName = "|Config ToSic.Eav.DataSources.SqlDataSource";
//        private const string DemoSql = @"/****** Script for SelectTopNRows command from SSMS  ******/
//SELECT TOP (1000) PortalId as EntityId, HomeDirectory as EntityTitle
//      ,[PortalID]
//      ,[ExpiryDate]
//      ,[AdministratorRoleId]
//      ,[GUID]
//      ,[DefaultLanguage]
//      ,[HomeDirectory]
//      ,[CreatedOnDate]
//      ,[PortalGroupID]
//  FROM [Portals]
//  Where ExpiryDate is null";

//        public ConfigSqlDataSource() : base(StaticTypeName, StaticTypeName, Constants.ScopeSystem, StaticTypeName.ToLowerInvariant().TrimStart('|'))
//        {
//            Add(AttDef(Str, DefInp, "Title", defaultValue: "Sql Query").MakeTitle());
//            Add(AttGrp("ConnectionGroup"));
//            {
//                Add(AttDef(Str, DefInp, "ConnectionStringName", defaultValue: "SiteSqlServer"));
//                Add(AttDef(Str, DefInp, "ConnectionString")).StringDefault(3);
//            }

//            Add(AttGrp("QueryGroup", "Query"));
//            { 
//                Add(AttDef(Str, DefInp, "SelectCommand", defaultValue: DemoSql)).StringDefault(10);
//                Add(AttDef(Str, DefInp, "ContentType", defaultValue: "SqlData")); 
//                Add(AttDef(Str, DefInp, "EntityIdField"));
//                Add(AttDef(Str, DefInp, "EntityTitleField"));
//            }

//            // this.ContentTypeMetadata("SQL DataSource");
//        }
//    }
//}
