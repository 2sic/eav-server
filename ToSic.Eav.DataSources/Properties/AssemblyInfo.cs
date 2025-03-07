using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// This is needed for unit testing
[assembly: InternalsVisibleTo("ToSic.Eav.UnitTests")]
[assembly: InternalsVisibleTo("ToSic.Testing.Shared")]
[assembly: InternalsVisibleTo("ToSic.Eav.Apps")]
[assembly: InternalsVisibleTo("ToSic.Eav.DataSources.Tests")]
[assembly: InternalsVisibleTo("ToSic.Eav.DataSource.TestHelpers")]