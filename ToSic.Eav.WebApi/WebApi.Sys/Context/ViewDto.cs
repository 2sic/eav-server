﻿using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Context;

public class ViewDto : IdentifierDto
{
    public required string Name;
    public string? Path;
    public required IEnumerable<ContentBlockDto> Blocks;

}