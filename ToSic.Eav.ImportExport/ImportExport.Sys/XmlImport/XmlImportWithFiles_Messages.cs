using ToSic.Eav.Persistence.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Internal;

partial class XmlImportWithFiles
{
    public List<Message> Messages;

    public string LogError(string message)
    {
        Log.A(message);
        Messages.Add(new(message, Message.MessageTypes.Error));
        return "error";
    }
    
}