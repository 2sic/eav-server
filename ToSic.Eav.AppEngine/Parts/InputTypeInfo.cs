namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Describes an input field type with it's labels, abilities etc. 
    /// This is so that input fields can self-describe.
    /// </summary>
    public class InputTypeInfo
    {
        public InputTypeInfo(string type, string label, string description, string assets, bool disableI18N)
        {
            Type = type;
            Label = label;
            Description = description;
            Assets = assets;
            DisableI18n = disableI18N;
        }
        public string Type { get; }
        public string Label { get; }
        public string Description { get; }
        public string Assets { get; }

        // ReSharper disable once InconsistentNaming
        public bool DisableI18n { get; }
    }
}
