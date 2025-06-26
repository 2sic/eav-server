using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Repositories.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class RepositoryBase(bool global, bool readOnly, RepositoryTypes type) : ServiceBase("RP.Info")
{
    public bool Global { get; } = global;

    public RepositoryTypes Type { get; } = type;

    public bool ReadOnly { get; } = readOnly;
}