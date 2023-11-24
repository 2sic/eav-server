using System;

namespace ToSic.Eav.Code.Infos;

public interface ICodeInfo
{
    string NameId { get; }
    Version From { get; }
    Version To { get; }
    string Link { get; }
    string Message { get; }

    CodeInfoTypes Type { get; }

    CodeUse UsedAs(int appId = default, string specificId = default, string[] more = default);

    ICodeInfo Replace(params object[] replacements);
}