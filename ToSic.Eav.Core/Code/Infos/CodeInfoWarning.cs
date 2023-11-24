using System;

namespace ToSic.Eav.Code.Infos;

public class CodeInfoWarning: CodeInfo
{
    private CodeInfoWarning(string nameId, Version from, Version to, string link = default, string message = default) : base(CodeInfoTypes.Warning, nameId, from, to, link, message)
    {
    }

    public static ICodeInfo Warn(string nameId, string link = default, string message = default) =>
        new CodeInfoWarning(nameId: nameId, from: new Version(), to: new Version(), link: link, message: message);


}