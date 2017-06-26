using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Interfaces;

namespace ToSic.Eav.Persistence
{

    public class SaveOptions
    {
        private SaveOptions()
        {
        }

        public static SaveOptions Build(int zoneId) => Factory.Resolve<IImportExportEnvironment>().SaveOptions(zoneId);

        public SaveOptions(string primaryLanguage, List<DimensionDefinition> languages)
        {
            PrimaryLanguage = primaryLanguage;
            Languages = languages;
        }

        public bool PreserveUntouchedAttributes = false;
        public bool PreserveUnknownAttributes = false;

        public bool SkipExistingAttributes = false;
        //public bool PreserveInvisibleAttributes = false;

        public string PrimaryLanguage
        {
            get
            {
                return _priLang ?? (_priLang = Factory.Resolve<IImportExportEnvironment>().DefaultLanguage);
            }
            set
            {
                _priLang = value.ToLowerInvariant();
            }
        }

        private string _priLang;
        public List<DimensionDefinition> Languages = null;
        public bool PreserveExistingLanguages = false;
        public bool PreserveUnknownLanguages = false;

        public bool DraftShouldBranch = true;

        /// <summary>
        /// Prepare relationships changes, but don't send to DB yet.
        /// This is important when saving many entities, as they could referr to each other and not exist yet when the first one is created. 
        /// So it will not apply relationships until the end of the transaction
        /// ...but you must remember to correctly trigger the relationship-save
        /// </summary>
        public bool DelayRelationshipSave = false;

    }
}
