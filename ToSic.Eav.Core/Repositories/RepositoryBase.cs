using ToSic.Lib.Services;

namespace ToSic.Eav.Repositories;

public abstract class RepositoryBase: ServiceBase
{
    protected RepositoryBase(bool global, bool readOnly, RepositoryTypes type) : base("RP.Info")
    {
        Global = global;
        ReadOnly = readOnly;
        Type = type;
    }
        

    public bool Global { get; }

    public RepositoryTypes Type { get; }

    public bool ReadOnly { get; }
}