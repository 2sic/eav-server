namespace ToSic.Eav.WebApi.Sys.Dto;

public class FileUploadDto
{
    public required string Name;
    public required Stream Stream;

    [JsonIgnore]
    [field: AllowNull, MaybeNull]
    public string Contents 
    {
        get
        {
            if (field != null)
                return field;
            using (var fileStreamReader = new StreamReader(Stream))
                field = fileStreamReader.ReadToEnd();
            return field;
        }
    }
}