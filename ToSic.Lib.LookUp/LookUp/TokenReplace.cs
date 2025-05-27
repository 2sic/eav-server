using System.Text;
using System.Text.RegularExpressions;

namespace ToSic.Eav.LookUp;

/// <summary>
/// The BaseTokenReplace class provides the tokenization of tokens formatted  
/// [object:property] or [object:property|format|ifEmpty] or [custom:no] within a string
/// with the appropriate current property/custom values.
/// </summary>
/// <remarks></remarks>
[PrivateApi("we might still rename this some day...")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class TokenReplace(ILookUpEngine lookupEngine)
{
    #region RegEx - the core formula
    // Commented Regular Expression which doesn't capture non-tokens
    // language=regex
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

    /// <summary>
    /// Gets the Regular expression for the token to be replaced
    /// </summary>
    /// <value>A regular Expression</value>   
    public static Regex Tokenizer { get; } = new(RegExFindAllTokens, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    #endregion

    public ILookUpEngine LookupEngine { get; } = lookupEngine ?? throw new("Can't initialize TokenReplace without engine");



    /// <summary>
    /// Checks for present [Object:Property] tokens
    /// </summary>
    /// <param name="sourceText">String with [Object:Property] tokens</param>
    /// <returns></returns>
    public static bool ContainsTokens(string sourceText)
    {
        if (string.IsNullOrEmpty(sourceText)) return false;
        foreach (Match currentMatch in Tokenizer.Matches(sourceText))
            if (currentMatch.Result("${object}").Length > 0)
                return true;
        return false;
    }

    /// <summary>
    /// Replace all tokens in a string. 
    /// </summary>
    /// <returns></returns>
    public virtual string ReplaceTokens(string sourceText, int repeat = 0, ITweakLookUp? tweaker = default)
    {
        if (string.IsNullOrEmpty(sourceText))
            return string.Empty;

        var result = new StringBuilder();
        var charProgress = 0;
        var matches = Tokenizer.Matches(sourceText);
            
        // If no matches found, just return the sourceText
        if (matches.Count <= 0) return sourceText;

        foreach (Match curMatch in matches)
        {
            // Get characters before the first match
            if (curMatch.Index > charProgress)
                result.Append(sourceText.Substring(charProgress, curMatch.Index - charProgress));
            charProgress = curMatch.Index + curMatch.Length;

            // get the infos we need to retrieve the value, get it. 
            var sourceName = curMatch.Result("${object}");
            if (string.IsNullOrEmpty(sourceName))
                continue;

            var specs = new LookUpSpecs(
                sourceName,
                curMatch.Result("${property}"),
                curMatch.Result("${format}"),
                curMatch.Result("${ifEmpty}")
            );
            var oneResult = RetrieveTokenValue(specs);

            var useFallback = string.IsNullOrEmpty(oneResult);
            if (useFallback)
                oneResult = specs.IfEmpty;
                        
            if (repeat > 0 || useFallback) // note: when using fallback, always re-run tokens, even if no repeat left
                oneResult = ReplaceTokens(oneResult, repeat - 1, tweaker);

            // Apply tweaker, but only if we got a reasonable result
            // Later we would probably want to improve this, and ask the source if it found a value (instead of empty-check)
            if (tweaker != null && !string.IsNullOrEmpty(oneResult))
                oneResult = tweaker.PostProcess(oneResult, specs: specs);

            result.Append(oneResult);
        }

        // attach the rest of the text (after the last match)
        result.Append(sourceText.Substring(charProgress));
                
        // Ready to finish, but first, ensure repeating if desired
        var finalResult = result.ToString();
        return finalResult;

    }

    /// <summary>
    /// Get a token value by checking all attached libraries of values and getting the right one
    /// </summary>
    /// <returns>
    /// The resulting string, formatted if necessary, or .
    /// </returns>
    private string RetrieveTokenValue(LookUpSpecs lookUpSpecs) 
        => LookupEngine.FindSource(lookUpSpecs.SourceName)?.Get(lookUpSpecs.Name, lookUpSpecs.Format) ?? string.Empty;
}