using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Constants related to Input Types
    /// </summary>
    [PrivateApi]
    public class InputTypes
    {
        /// <summary>
        /// Name of the content-type which describes Input-Types
        /// </summary>
        public const string TypeForInputTypeDefinition = "ContentType-InputType";


        public const string InputTypeType = "Type";
        public const string InputTypeLabel = "Label";
        public const string InputTypeDescription = "Description";
        public const string InputTypeAssets = "Assets";
        public const string InputTypeDisableI18N = "DisableI18n";
        public const string InputTypeAngularAssets = "AngularAssets";
        public const string InputTypeUseAdam = "UseAdam";

        public static string InputTypeWysiwyg = "string-wysiwyg";
    }
}
