using ToSic.Lib.Coding;
using ToSic.Sys.Code.Help;

namespace ToSic.Sys.Exceptions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class NamedArgumentException : ArgumentException, IExceptionWithHelp
{
    public NamedArgumentException(string message, string intro, string paramNames, string paramsText) :
        base(message)
    {
        Intro = intro;
        ParamNames = paramNames;
        ParamsText = paramsText;

        var help = new CodeHelp
        {
            Name = "named-parameters",
            Detect = null,
            LinkCode = NoParamOrder.HelpLink,
            UiMessage = " ",
            DetailsHtml = intro.Replace("\n", "<br>") +
                          (paramNames.HasValue() ? $"<br>Param Names: <code>{paramNames}</code>" : ""),
        };
        Helps = [help];
    }

    public string Intro { get; }
    public string ParamNames { get; }
    public string ParamsText { get; }
        
    public List<CodeHelp> Helps { get; }

}