namespace ToSic.Eav.Persistence.Logging
{
    /// <summary>
    /// Describes a Message while Exporting / Importing
    /// </summary>
    public partial class ExportImportMessage
    {
        public ExportImportMessage(string Message, ExportImportMessage.MessageTypes MessageType)
        {
            this.Message = Message;
            this.MessageType = MessageType;
        }

        public string Message { get; set; }
        public ExportImportMessage.MessageTypes MessageType { get; set; }
    }
}