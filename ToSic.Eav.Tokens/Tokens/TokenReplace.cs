#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

//using System.Globalization;

//using System.Threading;

//using DotNetNuke.Common.Utilities;

#endregion

//namespace DotNetNuke.Services.Tokens
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ToSic.Eav.ValueProviders;

namespace ToSic.Eav.Tokens
{
	/// <summary>
	/// The BaseTokenReplace class provides the tokenization of tokens formatted  
	/// [object:property] or [object:property|format|ifEmpty] or [custom:no] within a string
	/// with the appropriate current property/custom values.
	/// </summary>
	/// <remarks></remarks>
	public /* abstract */ class TokenReplace
    {
        #region RegEx - the core formula
        // 2dm 2015-03-09 new, commented version not capturing non-tokens
	    private const string RegExFindAllTokens = @"

# start by defining a group, but don't give it an own capture-name
(?:
# Every token must start with a square bracket
\[(?:
    # then get the object name, at least 1 char before a :, then followed by a :
    (?<object>[^\]\[:\s]+):
    # next get property key - can actually be very complex and include sub-properties; but it ends with a [,| or ]
    # note that after the first character it can contain : because sub-properties must be in this key
    (?<property>[^\]\[\|\s\:]+[^\]\[\|\s]*))
    # there may be more, but it's optional
    (?:
        # an optional format-parameter, it would be initiated by an |
        \|(?:(?<format>[^\]\[]*)
        # followed by another optional if-empty param, except that the if-empty can be very complex, containing more tokens
        \|(?:
            (?<ifEmpty>[^\[\}]+)
            # if ifEmpty contains more tokens, count open/close to make sure they are balanced
            |(?:
                (?<ifEmpty>\[(?>[^\[\]]+|\[(?<number>)|\](?<-number>))*(?(number)(?!))\])))
            )
        # not sure where this starts - or what it's for, but it's an 'or after a | you find a format...
        |\|(?:(?<format>[^\|\]\[]+))
    )?   # this packages is allowed 0 or 1 times so it ends with a ?
# and of course such a token must end with a ]
\])

";

        #endregion

        #region constructor
        public Dictionary<string, IValueProvider> ValueSources { get; }
	    public TokenReplace(Dictionary<string, IValueProvider> valueSources = null)
	    {
            if(valueSources == null)
                valueSources = new Dictionary<string, IValueProvider>(StringComparer.OrdinalIgnoreCase);
	        ValueSources = valueSources;
	    }
        #endregion



		/// <summary>
		/// Gets the Regular expression for the token to be replaced
		/// </summary>
		/// <value>A regular Expression</value>   
		public static Regex Tokenizer { get; } = new Regex(RegExFindAllTokens, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Checks for present [Object:Property] tokens
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens</param>
        /// <returns></returns>
        public static bool ContainsTokens(string sourceText)
        {
            if (!string.IsNullOrEmpty(sourceText))
            {
                foreach (Match currentMatch in Tokenizer.Matches(sourceText))
                    if (currentMatch.Result("${object}").Length > 0)
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Replace all tokens in a string. 
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="repeat"></param>
        /// <returns></returns>
        public virtual string ReplaceTokens(string sourceText, int repeat = 0)
        {
            if (string.IsNullOrEmpty(sourceText))
                return string.Empty;

            var Result = new StringBuilder();
            var charProgress = 0;
            var matches = Tokenizer.Matches(sourceText);
            if (matches.Count > 0)
            {
                foreach (Match curMatch in matches)
                {
                    // Get characters before the first match
                    if (curMatch.Index > charProgress)
                        Result.Append(sourceText.Substring(charProgress, curMatch.Index - charProgress));
                    charProgress = curMatch.Index + curMatch.Length;

                    // get the infos we need to retrieve the value, get it. 
                    var strObjectName = curMatch.Result("${object}");
                    if (!String.IsNullOrEmpty(strObjectName))
                    {
                        var strPropertyName = curMatch.Result("${property}");
                        var strFormat = curMatch.Result("${format}");
                        var strIfEmptyReplacment = curMatch.Result("${ifEmpty}");
                        var strConversion = RetrieveTokenValue(strObjectName, strPropertyName, strFormat);

                        var useFallback = string.IsNullOrEmpty(strConversion);
                        if (useFallback)
                            strConversion = strIfEmptyReplacment; 
                        
                        if (repeat > 0 || useFallback) // note: when using fallback, always re-run tokens, even if no repeat left
                            strConversion = ReplaceTokens(strConversion, repeat - 1);

                        Result.Append(strConversion);
                    }
                }

                // attach the rest of the text (after the last match)
                Result.Append(sourceText.Substring(charProgress));
                
                // Ready to finish, but first, ensure repeating if desired
                var finalResult = Result.ToString();
                //if (!repeatFallbackOnly && repeat > 0)
                //    finalResult = ReplaceTokens(finalResult, repeat - 1, repeatFallbackOnly);
                return finalResult;
            }

            // no matches found, just return the sourceText
            return sourceText;
        }

        /// <summary>
        /// Get a token value by checking all attached libraries of values and getting the right one
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="key"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        protected string RetrieveTokenValue(string sourceName, string key, string format)
        {
            var result = string.Empty;
            var propertyNotFound = false;
            if (ValueSources.ContainsKey(sourceName.ToLower()))
            {
                result = ValueSources[sourceName.ToLower()].Get(key, format, ref propertyNotFound);
            }
            return result;
        }
    }
}
