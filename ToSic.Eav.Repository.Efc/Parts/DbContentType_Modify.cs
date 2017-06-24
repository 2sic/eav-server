using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbContentType: BllCommandBase
    {

        public void AddOrUpdate(string staticName, string scope, string name, string description, int? usesConfigurationOfOtherSet, bool alwaysShareConfig, bool changeStaticName = false, string newStaticName = "")
        {
            var ct = GetTypeByStaticName(staticName);

            if (ct == null)
            {
                ct = new ToSicEavAttributeSets()
                {
                    AppId = DbContext.AppId,
                    StaticName = Guid.NewGuid().ToString(),// staticName,
                    Scope = scope == "" ? null : scope,
                    UsesConfigurationOfAttributeSet = usesConfigurationOfOtherSet,
                    AlwaysShareConfiguration = alwaysShareConfig
                };
                DbContext.SqlDb.Add(ct);
            }

            ct.Name = name;
            ct.Description = description;
            ct.Scope = scope;
            if (changeStaticName) // note that this is a very "deep" change
                ct.StaticName = newStaticName;
            ct.ChangeLogCreated = DbContext.Versioning.GetChangeLogId();

            // save first, to ensure it has an Id
            DbContext.SqlDb.SaveChanges();
        }



        public void Delete(string staticName)
        {
            var setToDelete = GetTypeByStaticName(staticName);

            setToDelete.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
            DbContext.SqlDb.SaveChanges();
        }

        
        public void ReorderAttributes(int contentTypeId, List<int> newSortOrder)
        {
            DbContext.AttributesDefinition.UpdateAttributeOrder(contentTypeId, newSortOrder);
        }
    }
}
