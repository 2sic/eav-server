// Global using directives

global using ToSic.Sys.Logging;
global using Xunit;
global using static Xunit.Assert;


// Fix issue with EditorBrowsableAttribute

//#if DEBUG
//[assembly: System.Reflection.AssemblyMetadata("IsTrimmable", "True")]
//#endif

// Can be removed when we convert projects to use new-style global usings
global using System.ComponentModel;

// Custom Test helpers