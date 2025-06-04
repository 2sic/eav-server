namespace ToSic.Eav.WebApi.Assets;

public class FileUploadDto
{
    public string Name;
    public Stream Stream;

    [JsonIgnore]
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