using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Persistence.Logging;
using ToSic.Lib.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport;

public partial class XmlImportWithFiles
{
    public List<Message> Messages;

    public string LogError(string message)
    {
        Log.A(message);
        Messages.Add(new Message(message, Message.MessageTypes.Error));
        return "error";
    }

    /// <summary>
    /// Maps EAV import messages to 2sxc import messages
    /// </summary>
    /// <param name="importLog"></param>
    /// <returns></returns>
    private IEnumerable<Message> GetExportImportMessagesFromImportLog(List<LogItem> importLog)
        => importLog.Select(l => new Message(l.Message,
            l.EntryType == EventLogEntryType.Error
                ? Message.MessageTypes.Error
                : l.EntryType == EventLogEntryType.Information
                    ? Message.MessageTypes.Information
                    : Message.MessageTypes.Warning
        ));
		

}