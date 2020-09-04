using System;

namespace ToSic.Eav.Run.Basic
{
    /// <summary>
    /// Trivial value converter - doesn't convert anything.
    /// </summary>
    public class BasicValueConverter : IValueConverter
    {
        public string ToReference(string value) => value;
        public string ToValue(string reference, Guid itemGuid) => reference;
    }
}
