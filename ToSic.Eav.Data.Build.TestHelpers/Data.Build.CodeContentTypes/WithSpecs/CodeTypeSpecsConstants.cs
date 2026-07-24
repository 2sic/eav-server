namespace ToSic.Eav.Data.Build.CodeContentTypes;

public class CodeTypeSpecsConstants
{
    #region Content Type Specs

    public const string SpecName = "TestTypeWithSpecsModified";
    public const string SpecGuid = "501ee043-1070-4cbc-a07b-8274f24bf5ea";
    public const string SpecScope = "DemoScope";
    public const string SpecDescription = "This is a test type with specs";

    #endregion

    #region Attribute Names

    public const string Url = "";
    public const string Age = "";
    public const string BirthDate = "";
    public const string IsAlive = "";
    public const string IgnoreThis = "";
    public const string InternalProperty = "";

    #endregion

    #region Attribute Specs

    public const string NameAttrSpecsNameModified = "NameMod"; // test that the name was changed
    public const string IdAndGuidDescription = "DO NOT USE. This is a temporary, random ID calculated at runtime and will return different values all the time.";

    /// <summary>
    /// The description is usually not public, but public here since the tests is elsewhere
    /// </summary>
    public const string IsAliveDescription = "This is to ensure the user is alive";

    #endregion
}