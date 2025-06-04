using ToSic.Eav.Data;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Tests;

internal static class EntitySaverTestAccessors
{
    public static IEntity TestCreateMergedForSavingTac(this EntitySaver saver, IEntity original, IEntity update, SaveOptions saveOptions, bool logDetails = true)
        => saver.CreateMergedForSaving(original, update, saveOptions, logDetails: logDetails);
}