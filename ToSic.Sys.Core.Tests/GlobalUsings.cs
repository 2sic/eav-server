// Global using directives

global using ToSic.Sys.Logging;
global using ToSic.Sys.Services;
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

