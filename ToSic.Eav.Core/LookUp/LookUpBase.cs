using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Base Class to create your own LookUp Class - used by all Look-Ups. <br/>
    /// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	public abstract class LookUpBase : ILookUp
    {
        #region default methods of interface
        /// <inheritdoc/>
        public string Name { get; protected set; }

        #region Sub-Token analysis and splitting
        // this is needed by some key accesses which support sub-properties like Content:Publisher:Location:City...
        // todo: should optimize to use named matches, to ensure that reg-ex changes doesn't change numbering...
        private static readonly Regex SubProperties = new Regex("([a-z]+):([a-z:]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// Check if it has sub-tokens and split up the material for further use
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        [PrivateApi]
        protected SubToken CheckAndGetSubToken(string original)
        {
            var result = new SubToken();

            // Do quick-check - without a ":" it doesn't have sub-tokens so stop here
            if (!original.Contains(":"))
                return result;

            var match = SubProperties.Match(original);
            if (match.Success)
            {
                result.HasSubtoken = true;
                result.Source = match.Groups[1].Value;
                result.Rest = match.Groups[2].Value;
            }
            return result;
        }

        
        #endregion


        /// <inheritdoc/>
	    public abstract string Get(string key, string format);

        /// <inheritdoc/>
	    public virtual string Get(string key) => Get(key, "");

        #endregion

        #region Helper functions
        /// <summary>
        /// Returns a formatted String if a format is given, otherwise it returns the unchanged value.
        /// </summary>
        /// <param name="value">string to be formatted</param>
        /// <param name="format">format specification</param>
        /// <returns>formatted string</returns>
        /// <remarks></remarks>
        [PrivateApi]
        public static string FormatString(string value, string format)
        {
            // if no format, don't convert
            if (string.IsNullOrWhiteSpace(format)) return value;
            // format if there was a value
            return string.IsNullOrEmpty(value) ? string.Format(format, value) : string.Empty;
        }

        public static string Format(bool value) => value.ToString().ToLowerInvariant();

        public static string Format(DateTime value) => value.ToUniversalTime()
            .ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        #endregion
    }

    [PrivateApi]
    public class SubToken
    {
        public bool HasSubtoken;
        public string Source;
        public string Rest;
    }
}
