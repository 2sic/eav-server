namespace ToSic.Eav.SysData;

/// <summary>
/// 
/// </summary>
/// <param name="Type">The type of requirement, such as `feature`</param>
/// <param name="NameId">The string identifier of this condition</param>
//[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record Requirement(string Type, string NameId);