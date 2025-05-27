using ToSic.Sys.Code.Help;

namespace ToSic.Sys.Exceptions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IExceptionWithHelp
{
    List<CodeHelp> Helps { get; }
}