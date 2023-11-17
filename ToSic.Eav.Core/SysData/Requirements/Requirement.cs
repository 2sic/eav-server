namespace ToSic.Eav.Configuration
{
    public class Condition
    {
        public Condition(string type, string nameId, bool isEnabled = true)
        {
            Type = type;
            NameId = nameId;
            IsEnabled = isEnabled;
        }

        public string Type { get; set; }

        /// <summary>
        /// The string identifier of this condition
        /// </summary>
        public string NameId { get; set; }

        /// <summary>
        /// Usually IsEnabled must be true
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
