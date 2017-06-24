using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbAttributeSet
    {

        private List<ToSicEavAttributeSets> _rememberSharedSets;
        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on specified App. 
        /// App must be saved and have a valid AppId
        /// </summary>
        internal void PrepareMissingSharedAttributesOnApp(ToSicEavApps app)
        {
            if (app.AppId == 0 || app.AppId < 0) // < 0 would be ef core un-saved temp-id
                throw new Exception("App must have a valid AppId");

            // don't test the Metadata app, waste of time
            if (app.AppId == Constants.MetaDataAppId)
                return;

            var sharedAttributeSets = _rememberSharedSets ?? (_rememberSharedSets = GetDbAttribSets(Constants.MetaDataAppId, null).Where(a => a.AlwaysShareConfiguration).ToList());
            var currentAppSharedSets = GetDbAttribSets(app.AppId, null).Where(a => a.UsesConfigurationOfAttributeSet.HasValue).Select(c => c.UsesConfigurationOfAttributeSet.Value).ToList();

            // test if all sets already exist
            var complete = true;
            sharedAttributeSets.ForEach(reqSet => complete = complete && currentAppSharedSets.Any(c => c == reqSet.AttributeSetId));

            if (complete)
                return;

            foreach (var sharedSet in sharedAttributeSets)
            {
                // create new AttributeSet - will be null if already exists
                var newOrNull = PrepareDbAttribSet(sharedSet.Name, sharedSet.Description, sharedSet.StaticName, sharedSet.Scope, /*false,*/ true, app.AppId);
                if (newOrNull != null)
                    newOrNull.UsesConfigurationOfAttributeSet = sharedSet.AttributeSetId;
            }
        }

        /// <summary>
        /// Ensure all AttributeSets with AlwaysShareConfiguration=true exist on all Apps an Zones
        /// </summary>
        public void EnsureSharedAttributeSetsOnEverythingAndSave()
        {
            foreach (var app in DbContext.SqlDb.ToSicEavApps)
                PrepareMissingSharedAttributesOnApp(app);

            DbContext.SqlDb.SaveChanges();
        }
    }
}
