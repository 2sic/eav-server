using System.Collections.Generic;

namespace ToSic.Eav.Data.PropertyLookup
{
    public class PropertyLookupPath
    {
        public PropertyLookupPath(List<string> original = null) 
            => Parts = original == null ? new List<string>() : new List<string>(original);

        public List<string> Parts;

    }


    public static class PropertyLookupPathExtensions
    {
        public static PropertyLookupPath KeepOrNew(this PropertyLookupPath original) 
            => original ?? new PropertyLookupPath();

        public static PropertyLookupPath Add(this PropertyLookupPath original, string next)
        {
            if (original == null) return null;
            original = new PropertyLookupPath(original.Parts);
            original.Parts.Add(next);
            return original;
        }
        public static PropertyLookupPath Add(this PropertyLookupPath original, string next, string field) 
            => original.Add($"{next}.{field}");

        public static PropertyLookupPath Add(this PropertyLookupPath original, string parent, string source, string field) 
            => original.Add($"{parent}[{source}].{field}");
    }
}
