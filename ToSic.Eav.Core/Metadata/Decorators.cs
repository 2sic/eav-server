﻿namespace ToSic.Eav.Metadata
{
    public class Decorators
    {
        public static string MessageField = "Message";

        // Is-Obsolete Decorator
        public static string IsObsoleteDecoratorId = "852fe15e-bf23-44e6-a856-0a130203496c";

        // Is-Default Decorator
        public static string IsDefaultDecorator = "529ba3a2-d7d4-4f40-a81b-ff819de03a9e";

        // Priority Decorator - not included ATM
        //public static string PriorityDecoratorId = "1291dbc0-9c52-4c79-8c95-698e5c3aa299";
        //public static string PriorityDefaultField = "IsDefault";

        // Is-Recommended Decorator
        public static string RecommendedDecoratorId = "c740085a-d548-41f3-8d06-0a48b8692345";

        // Informs what Metadata is expected / used on a specific item
        public static string MetadataExpectedDecoratorId = "c490b369-9cd2-4298-af74-19c1e438cdfc";

        public static string MetadataExpectedTypesField = "MetadataTypes";

        public static string MetadataForDecoratorId = "4c88d78f-5f3e-4b66-95f2-6d63b7858847";
        public static string MetadataForTargetTypeField = "TargetType";
        public static string MetadataForTargetNameField = "TargetName";
        public static string MetadataForDeleteWarningField = "DeleteWarning";

        // This marks entities which should allow saving empty
        public static string SaveEmptyDecoratorId = "8c78dabf-e0ad-4c26-b750-48138ecb8a39";

        // Marks Metadata to be global 13.00
        public static string IsSharedDecoratorId = "18d2b1db-e1ed-45bc-8746-cd8885651063";

        // Marks Requirements Metadata 13.00
        public static string RequirementsDecoratorId = "19655377-6626-4986-aea0-ec3c187186ad";
    }
}
