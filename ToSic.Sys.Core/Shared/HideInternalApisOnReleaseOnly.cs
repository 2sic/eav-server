
// This is a special helper file while developing
// It disables the official EditorBrowsableAttribute
// Because that is somehow broken ATM in Visual Studio
// Causing trouble while we're developing

// Fix issue with EditorBrowsableAttribute

#if DEBUG
global using ShowApiWhenReleased = FixEditorBrowsable.FakeEditorBrowsableAttribute;
global using ShowApiMode = FixEditorBrowsable.HideInternalApisOnReleaseOnly;
#else
global using ShowApiWhenReleased = System.ComponentModel.EditorBrowsableAttribute;
global using ShowApiMode = System.ComponentModel.EditorBrowsableState;
#endif



// ReSharper disable once CheckNamespace
namespace FixEditorBrowsable;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate | AttributeTargets.Interface)]
public sealed class FakeEditorBrowsableAttribute(FakeEditorBrowsableState state) : Attribute;

/// <summary>Specifies the browsable state of a property or method from within an editor.</summary>
public enum FakeEditorBrowsableState
{
    /// <summary>The property or method is always browsable from within an editor.</summary>
    Always,
    /// <summary>The property or method is never browsable from within an editor.</summary>
    Never,
    /// <summary>The property or method is a feature that only advanced users should see. An editor can either show or hide such properties.</summary>
    Advanced,
}