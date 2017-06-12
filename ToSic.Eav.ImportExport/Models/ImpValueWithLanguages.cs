using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpValueWithLanguages<T> : Value<T>, IValue /* 2017-06-12 2dm temp IImpValue */
    {
        //public T TypedContents { get; set; }
        //public List<ILanguage> Languages { get; set; } = new List<ILanguage>();
        //public ImpEntity ParentEntity { get; }

        public ImpValueWithLanguages(/*ImpEntity parentEntity,*/ T typedContents): base(typedContents)
        {
            //ParentEntity = parentEntity;
            //TypedContents = typedContents;
        }


        #region 2017-06-12 2dm disabled previously external stuff
        ///// <summary>
        ///// Append a language reference (ValueDimension) to this value (ValueImportModel).
        ///// </summary>
        //public void AddLanguageReference(string language, bool readOnly)
        //{
        //    // init real dimensions in case it's null...
        //    //if (Languages == null)
        //    //    Languages = new List<ILanguage>();

        //    if (!string.IsNullOrEmpty(language))
        //        Languages.Add( new Dimension { Key = language, ReadOnly = readOnly } );
        //}
        #endregion
    }
}