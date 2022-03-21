using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Testing.Shared
{
    public class TestHelpers
    {
        /// <summary>
        /// Compare two strings, and if not matching, will also report at what position the strings differed
        /// </summary>
        public static void Is(string expected, string result, string message = null)
        {
            var index = expected.Zip(result, (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;

            // if we found a deviation, include that in the message
            if (index <= result.Length)
            {
                var startExtract = index - 25;
                if (startExtract < 0) startExtract = 0;
                var length = index - startExtract;
                if (startExtract + length > expected.Length) length = expected.Length - startExtract;
                var before = expected.Substring(startExtract, length);

                message += $" (pos: {index}, before: '{before}')";
            }

            Assert.AreEqual(expected, result, message);
        }

    }
}
