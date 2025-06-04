namespace ToSic.Eav.Persistence.Sys.Logging;

/// <summary>
/// Describes a Message while Exporting / Importing
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class Message(string text, Message.MessageTypes messageType)
{
    public string Text { get; set; } = text;

    public MessageTypes MessageType { get; set; } = messageType;
}