namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Describes an input field type with it's labels, abilities etc. 
    /// This is so that input fields can self-describe.
    /// </summary>
    public class InputTypeInfo
    {
        public InputTypeInfo(string type, string label, string description, string assets, bool disableI18N, string ngAssets, bool useAdam)
        {
            Type = type;
            Label = label;
            Description = description;
            Assets = assets;
            DisableI18n = disableI18N;
            AngularAssets = ngAssets;
            UseAdam = useAdam;
        }
        public string Type { get; }
        public string Label { get; }
        public string Description { get; }
        public string Assets { get; }

        #region new in 2sxc 10 / eav 5

        // ReSharper disable once InconsistentNaming
        public bool DisableI18n { get; }

        public string AngularAssets { get; }


        public bool UseAdam { get; }

        #endregion

    }
}
