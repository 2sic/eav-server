using ToSic.Lib.Code.Help;

namespace ToSic.Eav;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IExceptionWithHelp
{
    List<CodeHelp> Helps { get; }
}