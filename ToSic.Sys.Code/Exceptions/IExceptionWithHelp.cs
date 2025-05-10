using ToSic.Lib.Code.Help;

namespace ToSic.Lib;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IExceptionWithHelp
{
    List<CodeHelp> Helps { get; }
}