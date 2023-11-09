using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Constants related to Input Types
    /// </summary>
    [PrivateApi]
    public class InputTypes: EntityBasedType
    {
        /// <summary>
        /// Name of the content-type which describes Input-Types
        /// </summary>
        public const string TypeForInputTypeDefinition = "ContentType-InputType";

        public InputTypes(IEntity entity) : base(entity)
        {
        }

        /// <summary>
        /// Optional CSV of custom configuration types instead of the default cascade
        /// </summary>
        public string CustomConfigTypes => GetThis(null as string);

        public string Label => GetThis(null as string);

        public string Description => GetThis(null as string);

        public string Assets => GetThis(null as string);

        public bool UseAdam => GetThis(false);

        public string AngularAssets => GetThis(null as string);

        // ReSharper disable once InconsistentNaming
        public bool DisableI18n => GetThis(false);

        public string Type => GetThis(null as string);
    }
}
