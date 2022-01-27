﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi.Languages
{
    public class LanguagesBackend: HasLog<LanguagesBackend>
    {
        #region Constructor & DI
        
        public LanguagesBackend(Lazy<IZoneMapper> zoneMapper, Lazy<ZoneManager> zoneManager, ISite site, Lazy<AppUserLanguageCheck> appUserLanguageCheckLazy) : base("Bck.Admin")
        {
            _zoneManager = zoneManager;
            _site = site;
            _appUserLanguageCheckLazy = appUserLanguageCheckLazy;
            _zoneMapper = zoneMapper;
        }
        private readonly Lazy<IZoneMapper> _zoneMapper;
        private readonly Lazy<ZoneManager> _zoneManager;
        private readonly ISite _site;
        private readonly Lazy<AppUserLanguageCheck> _appUserLanguageCheckLazy;

        #endregion

        public IList<SiteLanguageDto> GetLanguages()
        {
            var callLog = Log.Call();
            // ReSharper disable once PossibleInvalidOperationException
            var cultures = _zoneMapper.Value.Init(Log).CulturesWithState(_site)
                .Select(c => new SiteLanguageDto { Code = c.Code, Culture = c.Culture, IsEnabled = c.IsEnabled })
                .ToList();

            callLog("found:" + cultures.Count);
            return cultures;
        }

        public List<SiteLanguageDto> GetLanguagesOfApp(AppState appState, bool withCount = false)
        {
            try
            {
                var langs = _appUserLanguageCheckLazy.Value.Init(Log).LanguagesWithPermissions(appState);
                var converted = langs.Select(l =>
                    {
                        var dto = new SiteLanguageDto { Code = l.Code, Culture = l.Culture, IsAllowed = l.IsAllowed, IsEnabled = l.IsEnabled };
                        if (withCount) dto.Permissions = new HasPermissionsDto { Count = l.PermissionCount };
                        return dto;
                    })
                    .ToList();
                return converted;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return new List<SiteLanguageDto>();
            }

        }

        public void Toggle(string cultureCode, bool enable, string niceName)
        {
            Log.Add($"switch language:{cultureCode}, to:{enable}");
            // Activate or Deactivate the Culture
            _zoneManager.Value.Init(_site.ZoneId, Log).SaveLanguage(cultureCode, niceName, enable);
        }
    }
}