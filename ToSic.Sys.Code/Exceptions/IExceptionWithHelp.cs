using ToSic.Lib.Code.Help;

namespace ToSic.Lib;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IExceptionWithHelp
{
    List<CodeHelp> Helps { get; }
}