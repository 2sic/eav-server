using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Links;

/// <summary>
/// Link resolver, primarily for the edit UI.
/// </summary>
public interface ILinkResolver : IWork<ILinkResolver.Parameters, LinkInfoDto>
{
    public record Parameters(string Hyperlink, string? ContentType, Guid Entity, string? Field);

}
