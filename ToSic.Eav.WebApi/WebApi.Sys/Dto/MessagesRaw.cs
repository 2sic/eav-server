using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "Messages",
    Guid = "41bc9f69-6760-4cab-9004-6c848ed2e569",
    Description = "System message statistics",
    Scope = "System"
)]
public record MessagesRaw(
    int WarningsOther,
    int WarningsObsolete
) : IRawEntityAutoConvert;
