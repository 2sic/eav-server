using System;

namespace ToSic.Eav.Code
{
    public class NamedArgumentException : ArgumentException
    {
        public NamedArgumentException(string message, string intro, string paramNames, string paramsText) :
            base(message)
        {
            Intro = intro;
            ParamNames = paramNames;
            ParamsText = paramsText;
        }

        public string Intro { get; }
        public string ParamNames { get; }
        public string ParamsText { get; }
    }
}