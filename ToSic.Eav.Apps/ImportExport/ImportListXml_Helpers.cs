using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class ImportListXml
    {

        #region Timing / Debuging infos

        /// <summary>
        /// Helper to measure time used for stuff
        /// </summary>
        public Stopwatch Timer { get; set; } = new Stopwatch();

        public long TimeForMemorySetup;
        public long TimeForDbImport;
        #endregion

        private int _appId;

        /// <summary>
        /// The xml document to imported.
        /// </summary>
        public XDocument Document { get; private set; }

        /// <summary>
        /// The elements of the xml document.
        /// </summary>
        public IEnumerable<XElement> DocumentElements { get; private set; }

        private string _docLangPrimary;

        private IList<string> _languages;

        protected bool ResolveLinks;

        private ImportDeleteUnmentionedItems _deleteSetting;




        /// <summary>
        /// The entities created from the document. They will be saved to the repository.
        /// </summary>
        public List<Entity> ImportEntities { get; set; }

        private Entity GetImportEntity(Guid entityGuid)
        {
            var result = ImportEntities.FirstOrDefault(entity => entity.EntityGuid == entityGuid);
            if (result != null) Log.A($"Will modify entity from existing import list {entityGuid}");
            return result;
        }


        private int _appendEntityCount = 0;
        private Entity AppendEntity(Guid entityGuid)
        {
            if(_appendEntityCount++ < 100)
                Log.A($"Add entity to import list {entityGuid}");
            if (_appendEntityCount == 100) Log.A("Add entity: will stop listing each one...");
            if (_appendEntityCount % 100 == 0) Log.A("Add entity: Current count:" + _appendEntityCount);
            var entity = new Entity(_appId, entityGuid, ContentType, new Dictionary<string, object>());
            ImportEntities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Errors found while importing the document to memory.
        /// </summary>
        public ImportErrorLog ErrorLog { get; set; }


        private List<Guid> GetCreatedEntityGuids()
            => ImportEntities.Select(entity => entity.EntityGuid != Guid.Empty ? entity.EntityGuid : Guid.NewGuid()).ToList();


        /// <summary>
        /// Get the attribute names in the xml document.
        /// </summary>
        public IEnumerable<string> Info_AttributeNamesInDocument => DocumentElements.SelectMany(element => element.Elements())
            .GroupBy(attribute => attribute.Name.LocalName)
            .Select(group => group.Key)
            .Where(name => name != XmlConstants.EntityGuid && name != XmlConstants.EntityLanguage)
            .ToList();


        private IEntity FindInExisting(Guid guid)
            => ExistingEntities.FirstOrDefault(e => e.EntityGuid == guid);



    }

}
