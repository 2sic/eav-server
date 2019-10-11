using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles: HasLog
	{
        private List<DimensionDefinition> _targetDimensions;
        private DbDataController _eavContext;
		public int AppId { get; private set; }
		public int ZoneId { get; private set; }

        private readonly IImportExportEnvironment _environment;

	    private XmlToEntity _xmlBuilder;

        /// <summary>
        /// The default language / culture - example: de-DE
        /// </summary>
        private string DefaultLanguage { get; }

        private bool AllowUpdateOnSharedTypes { get; }


	    /// <summary>
	    /// Create a new xmlImport instance
	    /// </summary>
	    /// <param name="parentLog"></param>
	    /// <param name="defaultLanguage">The portals default language / culture - example: de-DE</param>
	    /// <param name="allowUpdateOnSharedTypes">Specify if the import should be able to change system-wide things like shared attributesets</param>
	    public XmlImportWithFiles(Log parentLog, string defaultLanguage = null, bool allowUpdateOnSharedTypes = false): base("Xml.ImpFil", parentLog)
		{
		    _environment = Factory.Resolve<IImportExportEnvironment>();
            _environment.LinkLog(Log);
			// Prepare
			Messages = new List<Message>();
		    DefaultLanguage = (defaultLanguage ?? _environment.DefaultLanguage).ToLowerInvariant();
			AllowUpdateOnSharedTypes = allowUpdateOnSharedTypes;
        }

	}
}