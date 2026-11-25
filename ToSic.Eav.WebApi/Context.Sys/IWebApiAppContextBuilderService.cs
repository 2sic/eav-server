namespace ToSic.Eav.Context.Sys;

public interface IWebApiAppContextBuilderService
{
    IContextOfApp? TryFindAppFromPath(string? nameOrPath);

    IContextOfApp? TryFindAppFromUrlIds();
}