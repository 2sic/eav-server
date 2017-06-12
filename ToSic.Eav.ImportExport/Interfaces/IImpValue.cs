using System.Collections.Generic;
using ToSic.Eav.ImportExport.Models;

namespace ToSic.Eav.ImportExport.Interfaces
{
    public interface IImpValue
    {
        IList<ILanguage> Languages { get; /*set;*/ }
        //ImpEntity ParentEntity { get; }

        // 2017-06-12 2dm disabled
        //void AddLanguageReference(string language, bool readOnly);
    }
}