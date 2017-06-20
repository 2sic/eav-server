using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Repository.Efc.Parts
{

    public class SaveOptions
    {
        //public SaveTypes Mode = SaveTypes.Update;
        public bool PreserveExistingAttributes = false;
        public bool PreserveUnknownAttributes = false;
        //public bool PreserveInvisibleAttributes = false;

        public string PrimaryLanguage { get { return _priLang; } set { _priLang = value.ToLowerInvariant(); } }
        private string _priLang = null;
        public List<ILanguage> Languages = null;
        public bool PreserveExistingLanguages = false;
        public bool PreserveUnknownLanguages = false;

        public bool ForceNoBranche = false;

        /// <summary>
        /// Prepare relationships changes, but don't send to DB yet.
        /// This is important when saving many entities, as they could referr to each other and not exist yet when the first one is created. 
        /// So it will not apply relationships until the end of the transaction
        /// ...but you must remember to correctly trigger the relationship-save
        /// </summary>
        public bool DelayRelationshipSave = false;

    }
}
