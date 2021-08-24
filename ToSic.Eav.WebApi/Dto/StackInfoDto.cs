namespace ToSic.Eav.WebApi.Dto
{
    public class StackInfoDto
    {
        public string Source { get; set; }
        
        public int Priority { get; set; }
        
        public string Path { get; set; }

        public object Value { get; set; }

        public string Type { get; set; }

        public int TotalResults { get; set; }
    }
}
