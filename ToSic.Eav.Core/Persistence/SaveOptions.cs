using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Interfaces;

namespace ToSic.Eav.Persistence
{

    public class SaveOptions
    {
        /// <summary>
        /// This makes sure that SaveOptions cannot be built directly, without
        /// understanding the consequences
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private SaveOptions() { }

        public static SaveOptions Build(int zoneId) => Factory.Resolve<IImportExportEnvironment>().SaveOptions(zoneId);

        public SaveOptions(string primaryLanguage, List<DimensionDefinition> languages)
        {
            PrimaryLanguage = primaryLanguage;
            Languages = languages;
        }

        public bool PreserveUntouchedAttributes = false;
        public bool PreserveUnknownAttributes = false;

        public bool SkipExistingAttributes = false;

        public string PrimaryLanguage
        {
            get => _priLang ?? (_priLang = Factory.Resolve<IImportExportEnvironment>().DefaultLanguage);
            set => _priLang = value.ToLowerInvariant();
        }

        private string _priLang;
        public List<DimensionDefinition> Languages = null;
        public bool PreserveExistingLanguages = false;
        public bool PreserveUnknownLanguages = false;

        public bool DraftShouldBranch = true;

        /// <summary>
        /// 
        /// </summary>
        public bool DiscardattributesNotInType = false;

        public string LogInfo => $"save opts PUntouchedAt:{PreserveUntouchedAttributes}, " +
                                 $"PUnknownAt:{PreserveUnknownAttributes}, " +
                                 $"SkipExstAt:{SkipExistingAttributes}" +
                                 $"ExistLangs:{PreserveExistingLanguages}, " +
                                 $"UnknownLangs:{PreserveUnknownLanguages}, " +
                                 $"draft-branch:{DraftShouldBranch}, Lang1:{_priLang}, langs⋮{Languages?.Count}, " +
                                 $"DiscardAttsNotInType:{DiscardattributesNotInType}";
    }
}
