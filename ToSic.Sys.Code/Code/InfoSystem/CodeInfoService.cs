﻿using ToSic.Sys.Code.Infos;

namespace ToSic.Sys.Code.InfoSystem;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class CodeInfoService(LazySvc<CodeInfosInScope> scope) : ServiceBase("Lib.CodeCh")
{
    public void Warn(CodeUse? use)
    {
        if (use is null)
            return;
        var logged = WarnObsolete(use);
        scope.Value.AddObsolete(logged);
    }

    public void Warn(ICodeInfo? change)
        => Warn(change == null
            ? null
            : new CodeUse(change)
        );

    /// <summary>
    /// Quick helper to warn and return an object. 
    /// </summary>
    public T GetAndWarn<T>(ICodeInfo change, T result)
    {
        Warn(change);
        return result;;
    }
}