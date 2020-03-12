using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Core.Tests.LookUp.Tokens
{
    [TestClass]
    public class TokenReplaceTst
    {
        #region Test Values

        private readonly string[] ValidPureTokens = new[]
        {
            "[Source:Key]",
            "[Source:Key|format]",
            "[QueryString:ProductId||27]",
            "[QueryString:ProductId|test {3}|27]",
            "[QueryString:Something||[Module:ModuleId]]",
        };

        private readonly string[] ValidTokensWithSubTokens = new[]
        {
            "[AppSettings:Owner:Name]",
            "[AppSettings:Owner:Name||Nobody]",
            "[AppSettings:Owner:Name||[AppSettings:DefaultOwner:Name||Nobody]]"
        };

        private readonly string[] ValidMixedSingleTokens = new[]
        {
            "The param was [QueryString:Id]",
            "[QueryString:Id] was the Param",
            "The param should be [QueryString:Id] but maybe not",
            "Before [Source:Key|format] after",
            "Before [QuerString:param||fallback] after",
            "before [QueryString:value||[Default:key||[VeryDefault:VDKey]]] after text",
            @"Also another
example with multiline [But:OnlyASingle] Token
or not"
        };

        private readonly string[] ValidMixedMultiTokens = new[]
        {
            "The param was [QueryString:Id] or it is [QueryString:Something]",
            "[QueryString:Id] was the Param or this [Source:Key] or this [Kiss:This||[Or:This]]",
            @"Should also work with Multiline [QueryString:View]
Tokens or anything that 
spreads across many things
[Like:This] or like [ThisOrThat:That]
"
        };

        private string[] VeryComplexTemplates = new[]
        {
            @"Select * From Users Where UserId = [QueryString:UserName||[AppSettings:DefaultUserName||0]] or UserId = [AppSettings:RootUserId] or UserId = [Parameters:TestKey:Subkey||27]
and a token with sub-token for fallback [QueryString:Id||[Module:SubId]]
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should [token:key] again"
        };

        private readonly string[] InvalidTokens = new[]
        {
            "not a token",
            "[]",
            "[ ]",
            "[...]",
            "[:]",
            "[SourceWithoutKey]",
            "[:KeyWitohutSource]",
            "[Source With Spaces:Key]",
            "[ SourceStartingWithSpace:Key]",
            "[Source|format]",
            "[Open without close",
            "Close without open]",
            "[Open||fallback]",
            "[Source::SubkeyWithoutKey]",
            "[Source:Key||[InvalidFallback]",
            "[Source:Key||[Invalid:Fallback|]"
        };

        private readonly Dictionary<string, string> qsValues = new Dictionary<string,string>()
        {
            {"Id", "27"},
            {"View", "Details"}
        };
        private readonly Dictionary<string, string> srcValues = new Dictionary<string, string>()
        {
            {"Key", "Kabcd"},
            {"Value", "Whatever"},
            {"View", "Details"}
        };
        private readonly LookUpInDictionary QueryString = new LookUpInDictionary("QueryString");
        private readonly LookUpInDictionary Source = new LookUpInDictionary("Source");
        private readonly Dictionary<string, ILookUp> AllSources = new Dictionary<string, ILookUp>(); 

        #endregion

        #region General test objects and initializer/constructor

        private readonly Regex tokenRegEx = TokenReplace.Tokenizer;
         

        public TokenReplaceTst()
        {
            // Initialize the QueryString PropertyAccess
            foreach (var qsValue in qsValues)
                QueryString.Properties.Add(qsValue.Key,qsValue.Value);
            foreach (var srcValue in srcValues)
                Source.Properties.Add(srcValue.Key, srcValue.Value);
            AllSources.Add(QueryString.Name, QueryString);
            AllSources.Add(Source.Name, Source);
        }

        #endregion

        #region ContainsToken tests
        [TestMethod]
        public void TokenReplace_ContainsToken()
        {
            foreach (var validPureToken in ValidPureTokens)
                Assert.IsTrue(TokenReplace.ContainsTokens(validPureToken));

            foreach (var validMixedSingleToken in ValidMixedSingleTokens)
                Assert.IsTrue(TokenReplace.ContainsTokens(validMixedSingleToken));

            foreach (var validMixedMultiToken in ValidMixedMultiTokens)
                Assert.IsTrue(TokenReplace.ContainsTokens(validMixedMultiToken));
        }

        [TestMethod]
        public void TokenReplace_ContainsTokenWithSubtoken()
        {
            foreach (var token in ValidTokensWithSubTokens)
                Assert.IsTrue(TokenReplace.ContainsTokens(token));   
        }

        [TestMethod]
        public void TokenReplace_DoesntContainToken()
        {
            foreach (var invalidToken in InvalidTokens)
                Assert.IsFalse(TokenReplace.ContainsTokens(invalidToken));
        }

        #endregion

        #region Test Simple Token Detection

        [TestMethod]
        public void TokenReplace_ValidPureTokens()
        {
            foreach (var testToken in ValidPureTokens)
            {
                var results = tokenRegEx.Matches(testToken);
                Assert.IsTrue(results.Count == 1, "Found too many results (" + results.Count + ") for token: " + testToken);
                Assert.IsTrue(results[0].Result("${object}").Length > 0,
                    "The group is of the wrong type, ${object}, but that's empty: " + testToken);
            }
        }

        [TestMethod]
        public void TokenReplace_ValidMixedSingleTokens()
        {
            foreach (var testToken in ValidMixedSingleTokens)
            {
                var results = tokenRegEx.Matches(testToken);
                Assert.IsTrue(results.Count == 1, "Should have found 1 placeholders: " + testToken);

                // Check that it only has 1 token match
                var count = CountSpecificResultTypes(results, "${object}");
                Assert.IsTrue(count == 1, "didn't find expected amount of ${object} should have found 1, found " + count + ": " + testToken);

            }
        }

        [TestMethod]
        public void TokenReplace_ValidMixedMultiTokens()
        {
            foreach (var testToken in ValidMixedMultiTokens)
            {
                var results = tokenRegEx.Matches(testToken);
                Assert.IsTrue(results.Count > 1, "Should have found more than 1 placeholders: " + testToken);

                // Check that it only has 1 token match
                var count = CountSpecificResultTypes(results, "${object}");
                Assert.IsTrue(count > 1, "didn't find expected amount of ${object} should have found 1, found " + count + ": " + testToken);

            }
        }

        [TestMethod]
        public void TokenReplace_InvalidTokens()
        {
            foreach (var testToken in InvalidTokens)
            {
                var results = tokenRegEx.Matches(testToken);

                var count = CountSpecificResultTypes(results, "${object}");
                Assert.IsTrue(count == 0, "Shouldn't find object - but did: " + testToken);
                
            }
        }
        #endregion

        #region Simple Replace Tests
        [TestMethod]
        public void TokenReplace_SimpleReplaceOnValidPureTokens()
        {
            // since the test string contains only the token, it should leave only this token as a result
            foreach (var token in ValidPureTokens)
            {
                var result = tokenRegEx.Replace(token, "12345");
                Assert.IsTrue(result == "12345", "Result should be 12345, apparently it isn't. Token was " + token + ", result was: " + result);
            }
        }
        #endregion

        #region Complex Replace Tests with 0 or 2 recurrances

        [TestMethod]
        public void TokenReplace_ComplexReplace()
        {
            var original =
                @"Select * From Users Where UserId = [QueryString:UserName||[AppSettings:DefaultUserName||0]] or UserId = [AppSettings:RootUserId] or UserId = [Parameters:TestKey:Subkey||27]
and a token with sub-token for fallback [QueryString:Id||[Module:SubId]]
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should [token:key] again
Now try a token which returns a token: [AppSettings:UserNameMaybeFromUrl||Johny]";
            
            // Even without recurrence it should process the fallback-token at least once
            var expectedNoRecurrance =
                @"Select * From Users Where UserId = Daniel or UserId = -1 or UserId = 27
and a token with sub-token for fallback 4567
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should What a Token! again
Now try a token which returns a token: [QueryString:UserName||Samantha]";

            var expectedRecurrance =
                @"Select * From Users Where UserId = Daniel or UserId = -1 or UserId = 27
and a token with sub-token for fallback 4567
some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either nor should [ something

and a [bad token without property] and a [Source::SubkeyWithoutKey]
but this should What a Token! again
Now try a token which returns a token: Daniel";

            var qs = new LookUpInDictionary("QueryString");
            qs.Properties.Add("UserName", "Daniel");
            //qs.Properties.Add("Id", "7");
            var mod = new LookUpInDictionary("Module");
            mod.Properties.Add("SubId", "4567");
            var appS = new LookUpInDictionary("AppSettings");
            appS.Properties.Add("DefaultUserName", "Name Unknown");
            appS.Properties.Add("RootUserId", "-1");
            appS.Properties.Add("UserNameMaybeFromUrl", "[QueryString:UserName||Samantha]");
            var tok = new LookUpInDictionary("token");
            tok.Properties.Add("key", "What a Token!");
            //var sources = new Dictionary<string, ILookUp>
            //{
            //    {qs.Name.ToLower(), qs},
            //    {mod.Name.ToLower(), mod},
            //    {appS.Name.ToLower(), appS},
            //    {tok.Name.ToLower(), tok}
            //};
            var engine = new LookUpEngine(null);
            engine.Add(qs);
            engine.Add(mod);
            engine.Add(appS);
            engine.Add(tok);

            var tr = new TokenReplace(/*sources*/engine);
            var resultNoRecurrance = tr.ReplaceTokens(original);
            Assert.AreEqual(expectedNoRecurrance, resultNoRecurrance, "No Recurrance");
            var resultRecurrance = tr.ReplaceTokens(original, 2);
            Assert.AreEqual(expectedRecurrance, resultRecurrance, "With 2x looping");
        }

        #endregion


        //todo
        // test sub-tokens
        // test format
        // test clean replace

        /// <summary>
        /// Helper method to count how many times a certain type of result was returned
        /// </summary>
        /// <param name="results"></param>
        /// <param name="resultName"></param>
        /// <returns></returns>
        public int CountSpecificResultTypes(MatchCollection results, string resultName)
        {
            var count = 0;
            foreach (Match match in results)
                if (!String.IsNullOrEmpty(match.Result(resultName)))
                    count++;
            return count;
        }
    }
}
