namespace ToSic.Eav.WebApi.Dto
{
    public class AppDto
    {
        public int Id { get; set; }
        public bool IsApp { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public string AppRoot { get; set; }
        public bool IsHidden { get; set; }
        public int? ConfigurationId { get; set; }
        public int Items { get; set; }
        public string Thumbnail { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Determines if the App is global / should only use templates/resources in the global storage
        /// </summary>
        /// <remarks>New in 13.0</remarks>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// Determines if this app was inherited from another App
        /// </summary>
        public bool IsInherited { get; set; }
    }
}
