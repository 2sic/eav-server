﻿using ToSic.Sys.Code.Help;

namespace ToSic.Sys.Exceptions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExceptionWithHelp : Exception, IExceptionWithHelp
{
    public ExceptionWithHelp(CodeHelp help, Exception? inner = null)
        : base(help.ErrorMessage, inner)
    {
        Helps = [help];
    }

    public ExceptionWithHelp(List<CodeHelp> helps, Exception? inner = null)
        : base(helps.FirstOrDefault()?.ErrorMessage ?? "", inner)
    {
        Helps = helps;
    }

    public List<CodeHelp> Helps { get; }
        
}