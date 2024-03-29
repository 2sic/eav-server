﻿namespace ToSic.Eav.WebApi.Assets;

public class AllFilesDto
{
    public IEnumerable<AllFileDto> Files = new List<AllFileDto>();
}

public class AllFileDto
{
    public string Path;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Shared;
}