namespace ToSic.Sys.Security.Permissions;

public record MockPermission(string Condition, string Identity, string Grant) : IPermission
{
    public string Title { get; init; } = "mock-permission";

    public string Owner { get; init; } = "";
}
