using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Run
{
    public class BasicGetDefaultLanguage: IGetDefaultLanguage
    {
        public string DefaultLanguage => "en-us";
    }
}
