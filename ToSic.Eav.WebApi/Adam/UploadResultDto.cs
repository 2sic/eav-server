using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Adam
{
    public class UploadResultDto: AdamItemDtoBase
    {
        //public bool Success { get; set; }
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //public string Error { get; set; }
        //public string Name { get; set; }
        public int Id { get; set; }
        //public string Path { get; set; }

        //public string Type { get; set; }

    }
}