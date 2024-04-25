﻿using ToSic.Eav.WebApi.Assets;

namespace ToSic.Eav.WebApi.ApiExplorer;

public class AllApiFilesDto
{
    public IEnumerable<AllApiFileDto> Files = [];
}

public class AllApiFileDto : AllFileDto
{
    public string EndpointPath;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsCompiled;
}