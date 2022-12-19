using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Reference to source code.
    /// It contains the path to the file, the method name and the line in the code. 
    /// </summary>
    /// <remarks>
    /// This is a very internal plumbing-object important for referencing code in log entries.
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("This is just FYI")]
    public class CodeRef
    {
        /// <summary>
        /// Path to the code file
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Name of the method/property accessed
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Line of code where the code was running
        /// </summary>
        public readonly int Line;

        /// <summary>
        /// Automatic constructor.
        /// It should be called without params, so that the compiler automatically injects all values.
        /// </summary>
        /// <param name="cPath">auto pre filled by the compiler - path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - method name</param>
        /// <param name="cLine">auto pre filled by the compiler - code line number</param>
        public CodeRef([CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default)
        {
            Path = cPath;
            Name = cName;
            Line = cLine;
        }

        /// <summary>
        /// Manually create a <see cref="CodeRef"/> using an already available set of path/name/line.
        /// Mainly used for very internal APIs.
        /// </summary>
        /// <returns>A new <see cref="CodeRef"/> object.</returns>
        // ReSharper disable ExplicitCallerInfoArgument
        public static CodeRef Create(string cPath, string cName, int cLine) => new CodeRef(cPath, cName, cLine);
        // ReSharper restore ExplicitCallerInfoArgument

        /// <summary>
        /// Special helper to choose from an already existing <see cref="CodeRef"/> object
        /// or to create a new one if it was null.
        /// </summary>
        /// <param name="codeRef">Exiting <see cref="CodeRef"/> object or `null`</param>
        /// <param name="cPath">auto pre filled by the compiler - path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - method name</param>
        /// <param name="cLine">auto pre filled by the compiler - code line number</param>
        /// <returns></returns>
        public static CodeRef UseOrCreate(CodeRef codeRef, string cPath, string cName, int cLine) => codeRef ?? Create(cPath, cName, cLine);
    }
}
