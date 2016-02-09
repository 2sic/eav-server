using System;
using System.Linq;
using ToSic.Eav.Implementations.ValueConverter;

namespace ToSic.Eav.ImportExport.Refactoring.ValueConverter
{
    public class NeutralValueConverter : IEavValueConverter
    {
        public string Convert(ConversionScenario scenario, string type, string originalValue)
        {
            return originalValue;
        }

    }




}