// Global using directives

global using System;
global using ToSic.Sys.Documentation;
global using ToSic.Sys.Logging;
global using System.Diagnostics.CodeAnalysis;
global using ToSic.Sys.Coding;
global using ToSic.Sys.Data;
global using ToSic.Sys.DI;
global using ToSic.Sys.Services;

// This alias is very important to clearly mark situations
// where we explicitly want the standard System.IServiceProvider.
// This is because in 99% of all cases we don't want it,
// but instead want our own ISysServiceProvider which can handle stacking and overrides.
global using ISystemServiceProviderExplicit = System.IServiceProvider;