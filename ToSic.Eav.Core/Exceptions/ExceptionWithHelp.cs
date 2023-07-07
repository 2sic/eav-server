﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Code.Help;

namespace ToSic.Eav
{
    public class ExceptionWithHelp : Exception, IExceptionWithHelp
    {
        public ExceptionWithHelp(CodeHelp help, Exception inner = null) : base(help.ErrorMessage, inner)
        {
            Helps = new List<CodeHelp> { help };
        }

        public ExceptionWithHelp(List<CodeHelp> helps, Exception inner = null) : base(helps?.FirstOrDefault()?.ErrorMessage ?? "", inner)
        {
            Helps = helps;
        }

        public List<CodeHelp> Helps { get; }
        
    }
}