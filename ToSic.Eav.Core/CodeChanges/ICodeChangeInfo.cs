using System;

namespace ToSic.Eav.CodeChanges
{
    public interface ICodeChangeInfo
    {
        string NameId { get; }
        Version From { get; }
        Version To { get; }
        string Link { get; }
        string Message { get; }

        CodeChangeUse UsedAs(int appId = default, string specificId = default, string[] more = default);
    }
}