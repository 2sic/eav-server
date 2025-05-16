using System.Reflection;
using static ToSic.Sys.Assembly.SharedAssemblyInfo;

// Use the globally defined assembly version information in all projects
// This file lies in the ToSic.Eav.Core project and is used as linked in other EAV projects
// See: https://denhamcoder.net/2018/09/11/visual-studio-synchronize-a-version-number-across-multiple-assemblies/

// For this to work, the .csproj file must also have some <generate...> set to false

[assembly: AssemblyVersion(AssemblyVersion)]
[assembly: AssemblyFileVersion(AssemblyVersion)]
[assembly: AssemblyInformationalVersion(AssemblyVersion)]
[assembly: AssemblyProduct(Product)]
[assembly: AssemblyCompany(Company)]
[assembly: AssemblyCopyright(Copyright)]


// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Assembly;

/// <summary>
/// Contains information for all assemblies to use
/// </summary>
public static class SharedAssemblyInfo
{
    // Important: These must be constants!
    // This is because the version etc. may be compiled into Oqtane DLLs
    // which will run in the browser, and shouldn't have to include these DLLs to work
    // If it's a constant, the value will be added to the compiled code, so no real dependency will exist at runtime
    public const string AssemblyVersion = "19.99.00";
    public const string Company = "2sic internet solutions GmbH, Switzerland";
    public const string Product = "2sic Sys/Lib Core Parts";
    public const string Copyright = "Copyright AGPL © 2sic 2025";
}


