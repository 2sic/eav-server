using System.Runtime.CompilerServices;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Logging
{
    /// <summary>
    /// Reference to code - containing the path to the file, the method name and the line in the code. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class CodeRef
    {
        /// <summary>
        /// Path to the code file
        /// </summary>
        public string Path;

        /// <summary>
        /// Name of the method/property accessed
        /// </summary>
        public string Name;

        /// <summary>
        /// Line of code where the code was running
        /// </summary>
        public int Line;

        /// <summary>
        /// Default constructor to set the values
        /// </summary>
        public CodeRef(string cPath, string cName, int cLine)
        {
            Path = cPath;
            Name = cName;
            Line = cLine;
        }

        /// <summary>
        /// This constructor should be called without params, so that the compiler automatically injects all values.
        /// </summary>
        /// <param name="autoPickup">Dummy parameter just to have a different signature. </param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        // ReSharper disable once UnusedParameter.Local - 
        public CodeRef(bool autoPickup = true, 
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
        :this(cPath,cName,cLine) { }
    }
}
