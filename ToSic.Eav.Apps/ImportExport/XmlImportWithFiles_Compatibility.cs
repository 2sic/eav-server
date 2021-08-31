using System;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
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
		    Log.Add("is compatible check");
		    var rns = doc.Elements(XmlConstants.RootNode);
		    var rn = doc.Element(XmlConstants.RootNode);
			// Return if no Root Node "SexyContent"
			if (!rns.Any() || rn == null)
			{
				Messages.Add(new Message("The XML file you specified does not seem to be a 2sxc/EAV Export.", Message.MessageTypes.Error));
				return false;
			}
			// Return if Version does not match
			if (rn.Attributes().All(a => a.Name != XmlConstants.MinEnvVersion) || new Version(rn.Attribute(XmlConstants.MinEnvVersion).Value) > new Version(Deps._environment.ModuleVersion))
			{
				Messages.Add(new Message("This template or app requires version " + rn.Attribute(XmlConstants.MinEnvVersion).Value + " in order to work, you have version " + Deps._environment.ModuleVersion + " installed.", Message.MessageTypes.Error));
				return false;
			}

		    Log.Add("is compatible completed");
            return true;
		}


	}

}