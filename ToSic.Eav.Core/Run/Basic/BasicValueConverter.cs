using System;

namespace ToSic.Eav.Run.Basic
{
    /// <summary>
    /// Trivial value converter - doesn't convert anything.
    /// </summary>
    public class ValueConverterBase : IValueConverter
    {
        public const string PrefixPage = "page";
        public const string PrefixFile = "file";
        public const string Separator = ":";

        public string ToReference(string value) => value;

        public string ToValue(string reference, Guid itemGuid) => reference;

        public static bool CouldBeReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return false;
            if (!reference.Contains(Separator)) return false;
            if (reference.Length < 6) return false; // minimum "page|file:number"
            return true;
        }
    }
}
