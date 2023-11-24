using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Constants related to attribute metadata
/// </summary>
[PrivateApi]
public class AttributeMetadata
{
    /// <summary>
    /// The content-type which describes general settings of an attribute
    /// </summary>
    public static string TypeGeneral = "@All";
    public static string TypeString = "@String";
    public static string GeneralFieldInputType = "InputType";

    public static string MetadataFieldAllIsEphemeral = "IsEphemeral";
    public static string MetadataFieldAllFormulas = "Formulas";
}