﻿namespace ToSic.Eav.ImportExport.Refactoring.Options
{
    public static class EntityClearImportExtension
    {
        public static bool IsNone(this EntityClearImport option)
        {
            return option == EntityClearImport.None;
        }

        public static bool IsAll(this EntityClearImport option)
        {
            return option == EntityClearImport.All;
        }
    }
}