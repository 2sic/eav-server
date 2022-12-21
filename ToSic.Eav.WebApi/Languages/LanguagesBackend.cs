using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Context;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Security;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Languages
{
    public class LanguagesBackend: ServiceBase
    {
        #region Constructor & DI
        
        public LanguagesBackend(LazySvc<IZoneMapper> zoneMapper, LazySvc<ZoneManager> zoneManager, ISite site, LazySvc<AppUserLanguageCheck> appUserLanguageCheckLazy) 
            : base("Bck.Admin") =>
            ConnectServices(
                _zoneManager = zoneManager,
                _site = site,
                _appUserLanguageCheckLazy = appUserLanguageCheckLazy,
                _zoneMapper = zoneMapper
            );

        private readonly LazySvc<IZoneMapper> _zoneMapper;
        private readonly LazySvc<ZoneManager> _zoneManager;
        private readonly ISite _site;
        private readonly LazySvc<AppUserLanguageCheck> _appUserLanguageCheckLazy;

        #endregion

        public IList<SiteLanguageDto> GetLanguages()
        {
            var callLog = Log.Fn<IList<SiteLanguageDto>>();
            // ReSharper disable once PossibleInvalidOperationException
            var cultures = _zoneMapper.Value.CulturesWithState(_site)
                .Select(c => new SiteLanguageDto { Code = c.Code, Culture = c.Culture, IsEnabled = c.IsEnabled })
                .ToList();

            return callLog.Return(cultures, "found:" + cultures.Count);
        }

        public List<SiteLanguageDto> GetLanguagesOfApp(AppState appState, bool withCount = false)
        {
            try
            {
                var langs = _appUserLanguageCheckLazy.Value.LanguagesWithPermissions(appState);
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
                Log.Ex(ex);
                return new List<SiteLanguageDto>();
            }

        }

        public void Toggle(string cultureCode, bool enable, string niceName)
        {
            Log.A($"switch language:{cultureCode}, to:{enable}");
            // Activate or Deactivate the Culture
            _zoneManager.Value.SetId(_site.ZoneId).SaveLanguage(cultureCode, niceName, enable);
        }
    }
}
