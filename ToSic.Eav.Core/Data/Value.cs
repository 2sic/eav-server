using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    public class Value
    {
        public IList<ILanguage> Languages { get; set; }
    }
}