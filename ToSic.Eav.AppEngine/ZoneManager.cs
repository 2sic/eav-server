﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    public class ZoneManager : ZoneBase
    {
        #region Constructor and simple properties

        public ZoneManager(int zoneId) : base(zoneId)
        {
        }

        //internal ZoneManager(EavDbContext _dbContext) : base(0)
        //{
        //    EfcContext = _dbContext;
        //}

        internal DbDataController DataController => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId));
        private DbDataController _eavContext;

        //internal EfcRepository Repo => _repo ?? (_repo = EfcRepository.Instance(ZoneId));
        //private EfcRepository _repo;

        //private EavDbContext EfcContext;

        #endregion

        #region App management

        public void DeleteApp(int appId)
            => SystemManager.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId), true);

        public int CreateApp()
        {
            var app = DataController.App.AddApp(null, Guid.NewGuid().ToString());

            SystemManager.PurgeEverything();
            return app.AppId;
        }

        #endregion

        #region Zone Management

        public static int CreateZone(string name)
        {
            //var zoneId = ZoneRepo.Create(name);
            var zoneId = DbDataController.Instance().Zone.AddZone(name).Item1.ZoneId;
            SystemManager.PurgeEverything();
            return zoneId;
        }

        #endregion

        #region Language management


        public void SaveLanguage(string cultureCode, string cultureText, bool active)
        {
            DataController.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);
            LanguageCache.Remove(ZoneId);
        }


        private static readonly Dictionary<int, List<ZoneLanguagesTemp>> LanguageCache =
            new Dictionary<int, List<ZoneLanguagesTemp>>();

        // todo: move retrieval to an interface, then move this read to ZoneRuntime!!!
        public List<ZoneLanguagesTemp> Languages
        {
            get
            {
                // note: cache at this level (to not instantiate a DB controller !) 
                if (!LanguageCache.ContainsKey(ZoneId))
                {
                    LanguageCache[ZoneId] = DataController.Dimensions.GetLanguages()
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        .Select(d => new ZoneLanguagesTemp {Active = d.Active, TennantKey = d.ExternalKey})
                        .ToList();
                }
                return LanguageCache[ZoneId];
            }
        }


    #endregion

        
    }

    public class ZoneLanguagesTemp
    {
        public string TennantKey;
        public bool Active;
    }
}
