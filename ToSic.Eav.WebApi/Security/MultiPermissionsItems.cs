using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Security;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.Security
{
    public class MultiPermissionsItems: MultiPermissionsApp
    {
        protected List<IEntity> Items;

        #region Constructors and DI / Init

        public MultiPermissionsItems(IZoneMapper zoneMapper): base(zoneMapper) { }

        public MultiPermissionsItems Init(IInstanceContext context, IApp app, IEntity item, ILog parentLog) 
        {
            Init(context, app, parentLog, "Sec.MpItms");
            Items = new List<IEntity> {item};
            return this;
        }

        #endregion

        protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
            => Items.ToDictionary(i => i.EntityId.ToString(), BuildItemPermissionChecker);

        /// <summary>
        /// Creates a permission checker for an type in this app
        /// </summary>
        /// <returns></returns>
        private IPermissionCheck BuildItemPermissionChecker(IEntity item)
        {
            var wrap = Log.Call< IPermissionCheck>($"{item.EntityId}");
            // now do relevant security checks
            return wrap("ok", BuildPermissionChecker(item.Type, item));
        }
    }
}
