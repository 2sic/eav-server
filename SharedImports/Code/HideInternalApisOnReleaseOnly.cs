// This is a special helper file while developing
// It disables the official EditorBrowsableAttribute
// Because that is somehow broken ATM in Visual Studio
// Causing trouble while we're developing

#if !DEBUG

// Production situation, where we want the real attributes applied

global using ShowApiWhenReleased = System.ComponentModel.EditorBrowsableAttribute;
global using ShowApiMode = System.ComponentModel.EditorBrowsableState;

#else

// Development situation, where we want to use our own attribute to prevent Visual Studio
// from hiding APIs

global using ShowApiWhenReleased = FixEditorBrowsable.FakeEditorBrowsableAttribute;
global using ShowApiMode = FixEditorBrowsable.FakeEditorBrowsableState;

// Note: We must include `using System;` in this file, as it may be reused in projects which don't have that
// ReSharper disable once RedundantUsingDirective
using System;

// ReSharper disable UseSymbolAlias

// ReSharper disable once CheckNamespace
namespace FixEditorBrowsable;

// We need to tell resharper to leave this as is, since it's sometimes turned off
// in which case Resharper thinks it could be removed
// ReSharper disable UnusedMember.Global
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate | AttributeTargets.Interface)]
#pragma warning disable CS9113 // Parameter is unread.
internal sealed class FakeEditorBrowsableAttribute(FakeEditorBrowsableState state) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.

/// <summary>Specifies the browsable state of a property or method from within an editor.</summary>
internal enum FakeEditorBrowsableState
{
    /// <summary>The property or method is always browsable from within an editor.</summary>
    Always,
    /// <summary>The property or method is never browsable from within an editor.</summary>
    Never,
    /// <summary>The property or method is a feature that only advanced users should see. An editor can either show or hide such properties.</summary>
    Advanced,
}
#endif

