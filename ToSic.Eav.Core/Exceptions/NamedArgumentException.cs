﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Code.Help;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav
{
    public class NamedArgumentException : ArgumentException, IExceptionWithHelp
    {
        public NamedArgumentException(string message, string intro, string paramNames, string paramsText) :
            base(message)
        {
            Intro = intro;
            ParamNames = paramNames;
            ParamsText = paramsText;

            var help = new CodeHelp("named-parameters", null,
                Parameters.HelpLink,
                uiMessage: " ",
                detailsHtml: intro.Replace("\n", "<br>") +
                             (paramNames.HasValue() ? $"<br>Param Names: <code>{paramNames}</code>" : ""));
            Helps = new List<CodeHelp> { help };
        }

        public string Intro { get; }
        public string ParamNames { get; }
        public string ParamsText { get; }
        
        public List<CodeHelp> Helps { get; }

    }
}