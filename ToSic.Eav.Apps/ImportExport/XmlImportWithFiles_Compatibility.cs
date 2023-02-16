using System;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {
		public bool IsCompatible(XDocument doc)
		{
		    Log.A("is compatible check");
		    var rns = doc.Elements(XmlConstants.RootNode);
		    var rn = doc.Element(XmlConstants.RootNode);
			// Return if no Root Node "SexyContent"
			if (!rns.Any() || rn == null)
			{
				Messages.Add(new Message("The XML file you specified does not seem to be a 2sxc/EAV Export.", Message.MessageTypes.Error));
				return false;
			}
			// Return if Version does not match
			if (rn.Attributes().All(a => a.Name != XmlConstants.MinEnvVersion) || new Version(rn.Attribute(XmlConstants.MinEnvVersion).Value) > new Version(Services._environment.ModuleVersion))
			{
				Messages.Add(new Message("This template or app requires version " + rn.Attribute(XmlConstants.MinEnvVersion).Value + " in order to work, you have version " + Services._environment.ModuleVersion + " installed.", Message.MessageTypes.Error));
				return false;
			}

		    Log.A("is compatible completed");
            return true;
		}


	}

}