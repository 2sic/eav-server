using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ToSic.Eav.Apps")]
[assembly: InternalsVisibleTo("ToSic.Eav.Persistence")]
//[assembly: InternalsVisibleTo("ToSic.Eav.Persistence.Efc")]

//[assembly: InternalsVisibleTo("ToSic.Eav.Core.Tests")]
[assembly: InternalsVisibleTo("ToSic.Eav.Core.TestsBasic")]
//[assembly: InternalsVisibleTo("ToSic.Testing.Shared")]
//[assembly: InternalsVisibleTo("ToSic.Sxc.WebApi")]
//[assembly: InternalsVisibleTo("ToSic.Sxc")]

//[assembly: InternalsVisibleTo("ToSic.Eav.Testing.FullDbFixtures")]


// The build API must be able to access some internal stuff
[assembly: InternalsVisibleTo("ToSic.Eav.Data.Build")]