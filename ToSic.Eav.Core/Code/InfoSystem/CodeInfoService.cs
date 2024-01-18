using ToSic.Eav.Code.Infos;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Code.InfoSystem;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class CodeInfoService(LazySvc<CodeInfosInScope> scope) : ServiceBase(EavLogs.Eav + ".CodeCh")
{
    public void Warn(CodeUse use)
    {
        if (use is null) return;
        var logged = WarnObsolete(use);
        scope.Value.AddObsolete(logged);
    }

    public void Warn(ICodeInfo change) => Warn(change == null ? null : new CodeUse(change));

    /// <summary>
    /// Quick helper to warn and return an object. 
    /// </summary>
    public T GetAndWarn<T>(ICodeInfo change, T result)
    {
        Warn(change);
        return result;;
    }
}