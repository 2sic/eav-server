using System.Collections.Generic;
using ToSic.Eav.Code.Help;

namespace ToSic.Eav
{
    public interface IExceptionWithHelp
    {
        List<CodeHelp> Helps { get; }
    }
}
