using ToSic.Lib.Code.Help;

namespace ToSic.Eav;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ExceptionWithHelp : Exception, IExceptionWithHelp
{
    public ExceptionWithHelp(CodeHelp help, Exception inner = null) : base(help.ErrorMessage, inner)
    {
        Helps = [help];
    }

    public ExceptionWithHelp(List<CodeHelp> helps, Exception inner = null) : base(helps?.FirstOrDefault()?.ErrorMessage ?? "", inner)
    {
        Helps = helps;
    }

    public List<CodeHelp> Helps { get; }
        
}