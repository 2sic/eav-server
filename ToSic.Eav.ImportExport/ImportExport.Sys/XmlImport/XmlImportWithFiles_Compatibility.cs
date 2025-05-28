using System.Xml.Linq;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.Persistence.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Internal;

partial class XmlImportWithFiles
{
    public bool IsCompatible(XDocument doc)
    {
        var l = Log.Fn<bool>("is compatible check");
        var rns = doc.Elements(XmlConstants.RootNode);
        var rn = doc.Element(XmlConstants.RootNode);
        // Return if no Root Node "SexyContent"
        if (!rns.Any() || rn == null)
        {
            Messages.Add(new("The XML file you specified does not seem to be a 2sxc/EAV Export.", Message.MessageTypes.Error));
            return l.ReturnFalse("XML seems invalid");
        }
        // Return if Version does not match
        if (rn.Attributes().All(a => a.Name != XmlConstants.MinEnvVersion) || new Version(rn.Attribute(XmlConstants.MinEnvVersion).Value) > new Version(base.Services.Environment.ModuleVersion))
        {
            Messages.Add(new("This template or app requires version " + rn.Attribute(XmlConstants.MinEnvVersion).Value + " in order to work, you have version " + base.Services.Environment.ModuleVersion + " installed.", Message.MessageTypes.Error));
            return l.ReturnFalse("XML version check failed");
        }

        return l.ReturnTrue("is compatible completed");
    }


}