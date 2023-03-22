using ToSic.Eav.Data;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Repository.Efc.Tests
{
    internal static class TestExtensionMethods
    {
        public static IEntity TestCreateMergedForSaving(this EntitySaver saver, IEntity original, IEntity update,
            SaveOptions saveOptions,
            bool logDetails = true)
        {
            return saver.CreateMergedForSaving(original, update, saveOptions, logDetails: logDetails);
        }
    }
}
