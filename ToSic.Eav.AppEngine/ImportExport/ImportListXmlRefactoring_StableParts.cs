using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class ImportListXmlRefactoring
    {

        #region Timing / Debuging infos

        /// <summary>
        /// Helper to measure time used for stuff
        /// </summary>
        public Stopwatch Timer { get; set; } = new Stopwatch();

        public long TimeForMemorySetup;
        public long TimeForDbImport;
        #endregion

        private readonly int _appId;
        private readonly int _zoneId;



        /// <summary>
        /// The xml document to imported.
        /// </summary>
        public XDocument Document { get; private set; }

        /// <summary>
        /// The elements of the xml document.
        /// </summary>
        public IEnumerable<XElement> DocumentElements { get; private set; }

        private readonly string _documentLanguageFallback;

        private readonly IEnumerable<string> _languages;

        private readonly ImportResourceReferenceMode _resolveReferenceMode;

        private readonly ImportDeleteUnmentionedItems _deleteSetting;




        /// <summary>
        /// The entities created from the document. They will be saved to the repository.
        /// </summary>
        public List<Entity> ImportEntities { get; }
        private Entity GetImportEntity(Guid entityGuid) => ImportEntities
            .FirstOrDefault(entity => entity.EntityGuid == entityGuid);


        private Entity AppendEntity(Guid entityGuid)
        {
            var entity = new Entity(_appId, entityGuid, ContentType.StaticName, new Dictionary<string, object>());
            ImportEntities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Errors found while importing the document to memory.
        /// </summary>
        public ImportErrorLog ErrorLog { get; }




        // todo: warning: 2017-06-12 2dm - I changed this a bit, must check for side-effects
        private List<Guid> GetCreatedEntityGuids()
            => ImportEntities.Select(entity => entity.EntityGuid != Guid.Empty ? entity.EntityGuid : Guid.NewGuid()).ToList();


        /// <summary>
        /// Get the attribute names in the xml document.
        /// </summary>
        public IEnumerable<string> AttributeNamesInDocument => DocumentElements.SelectMany(element => element.Elements())
            .GroupBy(attribute => attribute.Name.LocalName)
            .Select(group => @group.Key)
            .Where(name => name != XmlConstants.EntityGuid && name != XmlConstants.EntityLanguage)
            .ToList();




    }
}
