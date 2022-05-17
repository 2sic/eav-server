﻿using System.Reflection;
using static ToSic.Eav.SharedAssemblyInfo;

// Use the globally defined assembly version information in all projects
// This file lies in the ToSic.Eav.Core project and is used as linked in other EAV projects
// See: https://denhamcoder.net/2018/09/11/visual-studio-synchronize-a-version-number-across-multiple-assemblies/

// For this to work, the .csproj file must also have some <generate...> set to false

[assembly: AssemblyVersion(AssemblyVersion)]
[assembly: AssemblyFileVersion(AssemblyVersion)]
[assembly: AssemblyInformationalVersion(ToSic.Eav.SharedAssemblyInfo.AssemblyVersion)] 
[assembly: AssemblyProduct(EavProduct)]
[assembly: AssemblyCompany(Company)]
[assembly: AssemblyCopyright(EavCopyright)] 