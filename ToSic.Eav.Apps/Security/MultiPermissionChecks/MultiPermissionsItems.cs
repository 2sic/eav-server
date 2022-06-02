﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Security;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Security
{
    public class MultiPermissionsItems: MultiPermissionsApp
    {
        #region Constructors and DI / Init

        public MultiPermissionsItems(Dependencies dependencies): base(dependencies) { }

        public MultiPermissionsItems Init(IContextOfSite context, IAppIdentity app, IEntity item, ILog parentLog) 
        {
            Init(context, app, parentLog, "Sec.MpItms");
            _items = new List<IEntity> {item};
            return this;
        }
        private List<IEntity> _items;

        #endregion

        protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
            => _items.ToDictionary(i => i.EntityId.ToString(), BuildItemPermissionChecker);

        /// <summary>
        /// Creates a permission checker for an type in this app
        /// </summary>
        /// <returns></returns>
        private IPermissionCheck BuildItemPermissionChecker(IEntity item)
        {
            var wrap = Log.Fn<IPermissionCheck>($"{item.EntityId}");
            // now do relevant security checks
            return wrap.ReturnAsOk(BuildPermissionChecker(item.Type, item));
        }
    }
}
