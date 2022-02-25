using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Zone;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IZoneController
    {
        /// <summary>
        /// Get all languages of the current zone, with activation info
        /// </summary>
        /// <returns></returns>
        IList<SiteLanguageDto> GetLanguages();

        /// <summary>
        /// Enable / disable a language in the EAV
        /// </summary>
        /// <returns></returns>
        void SwitchLanguage(string cultureCode, bool enable);

        /// <summary>
        /// Get a bunch of system-info to show in the Apps Management
        /// </summary>
        /// <returns></returns>
        SystemInfoSetDto GetSystemInfo();
    }
}