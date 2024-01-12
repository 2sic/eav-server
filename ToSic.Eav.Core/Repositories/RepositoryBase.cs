using ToSic.Lib.Services;

namespace ToSic.Eav.Repositories;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class RepositoryBase(bool global, bool readOnly, RepositoryTypes type) : ServiceBase("RP.Info")
{
    public bool Global { get; } = global;

    public RepositoryTypes Type { get; } = type;

    public bool ReadOnly { get; } = readOnly;
}