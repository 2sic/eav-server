// Global using directives

global using ToSic.Lib.Logging;
global using Xunit;
global using static Xunit.Assert;


// Fix issue with EditorBrowsableAttribute

//#if DEBUG
//global using ShowApiWhenReleased = FixEditorBrowsable.EditorBrowsableAttribute;
//global using ShowApiMode = FixEditorBrowsable.EditorBrowsableState;
//#else
//global using ShowApiWhenReleased = System.ComponentModel.EditorBrowsableAttribute;
//global using ShowApiMode = System.ComponentModel.EditorBrowsableState;
//#endif

