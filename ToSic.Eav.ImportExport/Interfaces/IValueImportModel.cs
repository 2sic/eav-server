using System;
using System.Collections.Generic;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport.Models;

namespace ToSic.Eav.ImportExport.Interfaces
{
    public interface IValueImportModel
    {
        IEnumerable<ImpDims> ValueDimensions { get; set; }
        ImpEntity ParentEntity { get; }

        String StringValueForTesting { get; }

        List<IValueImportModel> ToList();
        void AppendLanguageReference(string language, bool readOnly);
    }
}