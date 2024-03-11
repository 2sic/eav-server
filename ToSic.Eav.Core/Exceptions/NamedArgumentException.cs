using System;
using System.Collections.Generic;
using ToSic.Eav.Code.Help;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;

namespace ToSic.Eav;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class NamedArgumentException : ArgumentException, IExceptionWithHelp
{
    public NamedArgumentException(string message, string intro, string paramNames, string paramsText) :
        base(message)
    {
        Intro = intro;
        ParamNames = paramNames;
        ParamsText = paramsText;

        var help = new CodeHelp("named-parameters", null,
            NoParamOrder.HelpLink,
            uiMessage: " ",
            detailsHtml: intro.Replace("\n", "<br>") +
                         (paramNames.HasValue() ? $"<br>Param Names: <code>{paramNames}</code>" : ""));
        Helps = [help];
    }

    public string Intro { get; }
    public string ParamNames { get; }
    public string ParamsText { get; }
        
    public List<CodeHelp> Helps { get; }

}