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

        public static bool CouldBeReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return false;
            if (!reference.Contains(":")) return false;
            if (reference.Length < 6) return false; // minimum "page|file:number"
            return true;
        }
    }
}
