namespace ToSic.Eav.WebApi.Sys.Dto;

/// <summary>
/// A helper to provide data to entity-pickers in a strange format
/// The format is not ideal, but the JS currently expects these keys
/// Should be standardized someday, but for now it's ok
/// </summary>
public class EntityForPickerDto
{
    public int Id { get; set; }
    public Guid Value { get; set; }
    public string Text { get; set; }
}