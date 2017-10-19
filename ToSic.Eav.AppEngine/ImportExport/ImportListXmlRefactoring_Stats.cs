using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class ImportListXmlRefactoring
    {

        /// <summary>
        /// Get the languages found in the xml document.
        /// </summary>
        public IEnumerable<string> LanguagesInDocument => DocumentElements
            .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value)
            .Distinct();

        /// <summary>
        /// Get the attributes not imported (ignored) from the document to the repository.
        /// </summary>
        public IEnumerable<string> AttributeNamesNotImported
        {
            get
            {
                var existingAttributes = AttributeNamesInContentType;
                var creatdAttributes = AttributeNamesInDocument;
                return existingAttributes.Except(creatdAttributes);
            }
        }


        /// <summary>
        /// The amount of enities created in the repository on data import.
        /// </summary>
        public int AmountOfEntitiesCreated
        {
            get
            {
                var existingGuids = GetExistingEntityGuids();
                var createdGuids = GetCreatedEntityGuids();
                return createdGuids.Except(existingGuids).Count();
            }
        }

        /// <summary>
        /// The amount of enities updated in the repository on data import.
        /// </summary>
        public int AmountOfEntitiesUpdated
        {
            get
            {
                var existingGuids = GetExistingEntityGuids();
                var createdGuids = GetCreatedEntityGuids();
                return createdGuids.Count(guid => existingGuids.Contains(guid));
            }
        }

        private List<Guid> GetEntityDeleteGuids()
        {
            var existingGuids = GetExistingEntityGuids();
            var createdGuids = GetCreatedEntityGuids();
            return existingGuids.Except(createdGuids).ToList();
        }

        /// <summary>
        /// The amount of enities deleted in the repository on data import.
        /// </summary>
        public int AmountOfEntitiesDeleted => _deleteSetting == ImportDeleteUnmentionedItems.None ? 0 : GetEntityDeleteGuids().Count;



    }
}
