using System;

namespace ToSic.Eav.Documentation
{
    /// <summary>
    /// This attribute serves as metadata for other things to mark them as public APIs
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    // ReSharper disable once InconsistentNaming
    public class PublicApi_Stable_ForUseInYourCode : Attribute
    {
        public PublicApi_Stable_ForUseInYourCode() { }

        public PublicApi_Stable_ForUseInYourCode(string comment = null) { }

    }
}
