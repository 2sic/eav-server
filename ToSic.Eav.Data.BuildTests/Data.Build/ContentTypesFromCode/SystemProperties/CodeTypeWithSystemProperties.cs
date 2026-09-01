namespace ToSic.Eav.Data.Build.ContentTypesFromCode.SystemProperties;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

/// <summary>
/// Properties such as ID and Guid should not be used for attributes.
/// </summary>
public class CodeTypeWithSystemProperties
{
    #region System Properties - will not be treated as attributes

    public int Id { get; set; }

    public Guid Guid { get; set; }

    public DateTime Created { get; set; }

    public DateTime Modified { get; set; }

    #endregion
    
    

    #region Standard Properties - will be treated as attributes

    /// <summary>
    /// In this test, this is the only field that should result in an Attribute.
    /// </summary>
    public string Name { get; set; }

    #endregion

}