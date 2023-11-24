namespace ToSic.Eav.Persistence.Logging;

/// <summary>
/// Describes a Message while Exporting / Importing
/// </summary>
public partial class Message
{
    public Message(string text, MessageTypes messageType)
    {
        Text = text;
        MessageType = messageType;
    }

    public string Text { get; set; }

    public MessageTypes MessageType { get; set; }
}