using System.Collections.Generic;
using ToSic.Eav.ImportExport.Models;

namespace ToSic.Eav.ImportExport.Interfaces
{
    public interface IImpValue
    {
        List<ImpDims> ValueDimensions { get; set; }
        ImpEntity ParentEntity { get; }

        void AppendLanguageReference(string language, bool readOnly);
    }
}