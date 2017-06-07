using System.Collections.Generic;
using ToSic.Eav.ImportExport.Interfaces;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpValue<T> : IImpValue
    {
        public T Value { get; set; }
        public List<ImpDims> ValueDimensions { get; set; }
        public ImpEntity ParentEntity { get; }

        public ImpValue(ImpEntity parentEntity, T value)
        {
            ParentEntity = parentEntity;
            Value = value;
        }

        #region previously external stuff
        /// <summary>
        /// Append a language reference (ValueDimension) to this value (ValueImportModel).
        /// </summary>
        public void AppendLanguageReference(string language, bool readOnly)
        {
            // init real dimensions in case it's null...
            if (ValueDimensions == null)
                ValueDimensions = new List<ImpDims>();

            if (!string.IsNullOrEmpty(language))
                ValueDimensions.Add( new ImpDims { DimensionExternalKey = language, ReadOnly = readOnly } );
        }
        #endregion
    }
}