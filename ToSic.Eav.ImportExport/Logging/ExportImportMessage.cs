namespace ToSic.Eav.ImportExport.Logging
{
    /// <summary>
    /// Describes a Message while Exporting / Importing
    /// </summary>
    public partial class ExportImportMessage
    {
        public ExportImportMessage(string Message, MessageTypes MessageType)
        {
            this.Message = Message;
            this.MessageType = MessageType;
        }

        public string Message { get; set; }
        public MessageTypes MessageType { get; set; }
    }
}