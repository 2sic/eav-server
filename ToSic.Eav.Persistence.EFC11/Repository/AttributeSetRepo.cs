using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11.Repository
{
    public class AttributeSetRepo: EfcRepoPart
    {
        public AttributeSetRepo(EfcRepository parent) : base(parent) { }


        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on specified App. App must be saved and have an AppId
        /// </summary>
        internal void EnsureSharedAttributeSets(ToSicEavApps app, bool autoSave = true)
        {
            if (app.AppId == 0)
                throw new Exception("App must have a valid AppID");

            // todo: bad - don't want data-sources here
            var sharedAttributeSets = GetAttributeSets(Constants.MetaDataAppId, null).Where(a => a.AlwaysShareConfiguration);
            foreach (var sharedSet in sharedAttributeSets)
            {
                // Skip if attributeSet with StaticName already exists
                if (app.AttributeSets.Any(a => a.StaticName == sharedSet.StaticName && !a.ChangeLogIDDeleted.HasValue))
                    continue;

                // create new AttributeSet
                var newAttributeSet = AddContentTypeAndSave(sharedSet.Name, sharedSet.Description, sharedSet.StaticName, sharedSet.Scope, false, app.AppID);
                newAttributeSet.UsesConfigurationOfAttributeSet = sharedSet.AttributeSetID;
            }

            // Ensure new AttributeSets are created and cache is refreshed
            if (autoSave)
                Db.SaveChanges();
        }
    }
}
